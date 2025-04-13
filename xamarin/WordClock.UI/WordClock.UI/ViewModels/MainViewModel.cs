using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akavache;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Websockets;
using WordClock.UI.Models;

namespace WordClock.UI.ViewModels
{
    public class MainViewModel : ReactiveObject, IMainViewModel
    {
        private const int Port = 81;
        
        private readonly BehaviorSubject<int> _hostAddressSubject;
        private readonly IWebSocketConnection _connection;
        
        public MainViewModel()
        {
            _hostAddressSubject = new BehaviorSubject<int>(0);
            _connection = WebSocketFactory.Create();
            _connection.OnOpened += () => ConnectionState = ConnectionState.Connected();
            
            InitProperties();
            InitCachingOfIpAddress();
            HandleOpenSocketConnection();
        }

        private void InitProperties()
        {
            InitIpAddressProperty();
            InitColorProperties();
            InitIsNightModeProperties();
        }

        private void InitIpAddressProperty()
        {
            Observable
                .Defer(() => Observable.Return(GetTurncatedIpAddress()))
                .SelectMany(turncatedIp => BlobCache
                    .UserAccount
                    .GetObject<string>("ipAddress")
                    .Catch(Observable.Return($"{turncatedIp}.0"))
                    .Merge(_hostAddressSubject
                        .Skip(1)
                        .DoWhere(x => x == 256, _ => ConnectionState = ConnectionState.Failed("Couldn't connect to Clock"))
                        .Where(x => x < 256 && ConnectionState.Type != StateType.Connected)
                        .Select(x => $"{turncatedIp}.{x}")))
                .Catch<string, Exception>(HandleException<string>)
                .ToPropertyEx(this, x => x.IpAddress);
        }

        private IObservable<T> HandleException<T>(Exception e)
        {
            ConnectionState = ConnectionState.Failed(e.Message);
            return Observable.Empty<T>();
        }

        private static string GetTurncatedIpAddress()
        {
            var ip = Dns
                .GetHostEntry(Dns.GetHostName())
                .AddressList
                .AsEnumerable()
                .FirstOrDefault(GetIpAddressFilter)?
                .ToString();

            if (string.IsNullOrEmpty(ip)) {
                throw new IOException("Could't get Wifis IP address");
            } else {
                return ip?.Substring(0, ip.LastIndexOf(".", StringComparison.CurrentCulture));
            }
        }

        private static bool GetIpAddressFilter(IPAddress ipAddress)
        {
            return ipAddress.AddressFamily == AddressFamily.InterNetwork && 
                   ipAddress.ToString().StartsWith("192", StringComparison.CurrentCulture);
        }

        private void InitColorProperties()
        {
            BlobCache
                .UserAccount
                .GetObject<int>("colorA")
                .Catch(Observable.Return(32))
                .SubscribeSafe(x => Alpha = x);
            
            BlobCache
                .UserAccount
                .GetObject<int>("colorR")
                .Catch(Observable.Return(255))
                .SubscribeSafe(x => Red = x);

            BlobCache
                .UserAccount
                .GetObject<int>("colorG")
                .Catch(Observable.Return(255))
                .SubscribeSafe(x => Green = x);

            BlobCache
                .UserAccount
                .GetObject<int>("colorB")
                .Catch(Observable.Return(255))
                .SubscribeSafe(x => Blue = x);
            
            this.WhenAnyValue(x => x.Alpha, x => x.Red, x => x.Green, x => x.Blue, Color.FromArgb)
                .ToPropertyEx(this, x => x.Color)
                .ThrownExceptions
                .SubscribeSafe();

            this.WhenAnyValue(x => x.Color)
                .Where(_ => _connection.IsOpen)
                .Throttle(TimeSpan.FromMilliseconds(50))
                .Merge(GetConnectedSignal().Select(_ => Color))
                .Do(x => BlobCache.UserAccount.InsertObject("colorA", x.A).SubscribeSafe())
                .Do(x => BlobCache.UserAccount.InsertObject("colorR", x.R).SubscribeSafe())
                .Do(x => BlobCache.UserAccount.InsertObject("colorG", x.G).SubscribeSafe())
                .Do(x => BlobCache.UserAccount.InsertObject("colorB", x.B).SubscribeSafe())
                .Do(x => _connection.Send($"a{x.A}r{x.R}g{x.G}b{x.B}"))
                .SubscribeSafe();
        }
        
        private void InitIsNightModeProperties()
        {
            BlobCache
                .UserAccount
                .GetObject<bool>("isNightModeEnabled")
                .Catch(Observable.Return(false))
                .SubscribeSafe(x => IsNightModeEnabled = x);
            
            BlobCache
                .UserAccount
                .GetObject<int>("NightModeBrightness")
                .Catch(Observable.Return(1))
                .SubscribeSafe(x => NightModeBrightness = x);
            
            BlobCache
                .UserAccount
                .GetObject<TimeSpan>("NightModeFromTime")
                .Catch(Observable.Return(TimeSpan.FromHours(1)))
                .SubscribeSafe(x => NightModeFromTime = x);
            
            BlobCache
                .UserAccount
                .GetObject<TimeSpan>("NightModeToTime")
                .Catch(Observable.Return(TimeSpan.FromHours(8)))
                .SubscribeSafe(x => NightModeToTime = x);

            this.WhenAnyValue(x => x.IsNightModeEnabled)
                .Merge(GetConnectedSignal().Select(_ => IsNightModeEnabled))
                .Do(x => BlobCache.UserAccount.InsertObject("isNightModeEnabled", x).SubscribeSafe())
                .Do(x => _connection.Send($"nme{(x ? 1 : 0)}"))
                .SubscribeSafe();
            
            this.WhenAnyValue(x => x.NightModeBrightness)
                .Merge(GetConnectedSignal().Select(_ => NightModeBrightness))
                .Do(x => BlobCache.UserAccount.InsertObject("NightModeBrightness", x).SubscribeSafe())
                .Do(x => _connection.Send($"nb{x}"))
                .SubscribeSafe();
            
            this.WhenAnyValue(x => x.NightModeFromTime)
                .Merge(GetConnectedSignal().Select(_ => NightModeFromTime))
                .Do(x => BlobCache.UserAccount.InsertObject("NightModeFromTime", x).SubscribeSafe())
                .Do(x => _connection.Send($"nft{x.TotalMilliseconds}"))
                .SubscribeSafe();
            
            this.WhenAnyValue(x => x.NightModeToTime)
                .Merge(GetConnectedSignal().Select(_ => NightModeToTime))
                .Do(x => BlobCache.UserAccount.InsertObject("NightModeToTime", x).SubscribeSafe())
                .Do(x => _connection.Send($"ntt{x.TotalMilliseconds}"))
                .SubscribeSafe();
        }
        
        private void InitCachingOfIpAddress()
        {
            GetConnectedSignal()
                .SelectMany(_ => BlobCache.UserAccount.InsertObject("ipAddress", IpAddress))
                .SubscribeSafe();
        }

        private IObservable<Unit> GetConnectedSignal()
        {
            return this.WhenAnyValue(x => x.ConnectionState.Type)
                .Where(x => x == StateType.Connected)
                .ToSignal();
        }

        private void HandleOpenSocketConnection()
        {
            this.WhenAnyValue(x => x.IpAddress)
                .WhereNotNull()
                .SelectMany(x => OpenSocket(x)
                    .DoOnError(e => _hostAddressSubject.OnNext(_hostAddressSubject.Value + 1))
                    .Catch<Unit, TimeoutException>(e => Observable.Empty<Unit>()))
                .Catch<Unit, Exception>(HandleException<Unit>)
                .SubscribeSafe();
        }

        private IObservable<Unit> OpenSocket(string ipAddress) 
        {
            _connection.Open($"ws://{ipAddress}:{Port}/", "arduino");
            ConnectionState = ConnectionState.Connecting(ipAddress);
            return GetConnectedSignal()
                .Timeout(TimeSpan.FromSeconds(2))
                .TakeUntil(GetConnectedSignal());
        }
        
        private extern string IpAddress { [ObservableAsProperty] get; }
        
        [Reactive] public int Red { get; set; }
        [Reactive] public int Green { get; set; }
        [Reactive] public int Blue { get; set; }
        [Reactive] public int Alpha { get; set; }
        [Reactive] public bool IsNightModeEnabled { get; set; }
        [Reactive] public int NightModeBrightness { get; set; }
        [Reactive] public TimeSpan NightModeFromTime { get; set; }
        [Reactive] public TimeSpan NightModeToTime { get; set; }
        [Reactive] public ConnectionState ConnectionState { get; private set; } = ConnectionState.Connecting("");
        public extern Color Color { [ObservableAsProperty] get; }
    }
}
