using System;
using System.Collections;
using UnityEngine;
using AlgerBusSim.Core;
using AlgerBusSim.World;
using AlgerBusSim.Data;
using AlgerBusSim.Vehicles;
using AlgerBusSim.UI;
using AlgerBusSim.Localization;
using AlgerBusSim.Gameplay;

namespace AlgerBusSim.Gameplay
{
    public class RouteSession : MonoBehaviour
    {
        public class Summary
        {
            public long Earnings;
            public int Passengers;
            public int StopsServed;
            public int PunctualityHits;
            public int TotalStops;
        }

        private GameSession _session;
        private WorldLayout _layout;
        private RouteData _route;
        private EconomyData _econ;
        private BusController _bus;
        private GameHud _hud;

        private int _stopIndex = 0;
        private int _onboard = 0;
        private int _capacity = 85;
        private long _earned = 0;
        private int _carried = 0;
        private int _punctualityHits = 0;
        private bool _atStop = false;
        private bool _doorsWereOpen = false;
        private float _dwellTimer = 0f;
        private float _serviceStartHour;
        private float _elapsedSeconds;
        private float _nextMissedCheck = 0f;
        private StopPoint _currentStop;
        private bool _stopServed = false;
        private float _leaveTimer = 0f;

        public void Init(GameSession session)
        {
            _session = session;
            _layout = session.Layout;
            _route = session.Route;
            _econ = session.Economy;
            _bus = session.Controller;
            _hud = session.Hud;
            _serviceStartHour = session.DayNight.Hour;
            // prepare first stop
            AdvanceToStop(0);
            StartCoroutine(Loop());
        }

        private void AdvanceToStop(int idx)
        {
            _stopIndex = idx;
            if (idx < _layout.Stops.Count)
            {
                _currentStop = _layout.Stops[idx];
                _stopServed = false;
                _atStop = false;
                _dwellTimer = 0f;
                _leaveTimer = 0f;
            }
            else
            {
                _currentStop = null;
                Finish();
            }
        }

        private IEnumerator Loop()
        {
            while (_session.State == GameState.Driving)
            {
                _elapsedSeconds += Time.deltaTime;
                if (_currentStop != null)
                {
                    float dist = Vector3.Distance(_bus.transform.position, _currentStop.BusPosition);
                    _atStop = dist < 18f && Mathf.Abs(_bus.SpeedMs()) < 0.6f;

                    if (_atStop && !_stopServed)
                    {
                        if (!_bus.DoorSystem.DoorsOpen)
                        {
                            // hint
                            _hud.SetHint(AlgerBusSim.Localization.Localization.Get("open_doors_hint"));
                        }
                        else
                        {
                            _dwellTimer += Time.deltaTime;
                            _hud.SetHint(AlgerBusSim.Localization.Localization.Get("boarding") + "…");
                            if (_dwellTimer > 1.6f && !_stopServed)
                            {
                                ServeStop();
                            }
                        }
                    }

                    // missed stop check
                    if (!_stopServed && dist > 60f && _nextMissedCheck <= 0f)
                    {
                        // passed the stop
                        Vector3 toStop = _currentStop.BusPosition - _bus.transform.position;
                        if (Vector3.Dot(_bus.transform.forward, toStop) < -5f)
                        {
                            _nextMissedCheck = 5f;
                            _earned -= _econ.penalties.missedStopDzd;
                            _hud.Toast(AlgerBusSim.Localization.Localization.Get("penalty_missed_stop") + " -" + _econ.penalties.missedStopDzd);
                            AdvanceToStop(_stopIndex + 1);
                        }
                    }
                    if (_nextMissedCheck > 0f) _nextMissedCheck -= Time.deltaTime;

                    // leave after served + doors closed + moved
                    if (_stopServed)
                    {
                        if (_bus.DoorSystem.IsClosed())
                        {
                            if (Vector3.Distance(_bus.transform.position, _currentStop.BusPosition) > 25f)
                            {
                                _leaveTimer += Time.deltaTime;
                                if (_leaveTimer > 0.5f) AdvanceToStop(_stopIndex + 1);
                            }
                        }
                        else
                        {
                            _hud.SetHint(AlgerBusSim.Localization.Localization.Get("close_doors_hint"));
                        }
                    }
                }

                // speed penalty
                CheckSpeeding();

                // red light penalty
                if (_layout.Traffic != null)
                {
                    if (_layout.Traffic.IsRedFor(_bus.transform.position, _bus.transform.forward) &&
                        Mathf.Abs(_bus.SpeedKmh()) > 15f && Vector3.Distance(_bus.transform.position, NearestJunction()) < 18f)
                    {
                        // penalize once per 8s
                        if (_redLightCooldown <= 0f)
                        {
                            _redLightCooldown = 8f;
                            _earned -= _econ.penalties.redLightDzd;
                            _hud.Toast(AlgerBusSim.Localization.Localization.Get("penalty_red_light") + " -" + _econ.penalties.redLightDzd);
                        }
                    }
                }
                if (_redLightCooldown > 0f) _redLightCooldown -= Time.deltaTime;

                // update HUD
                _hud.SetSpeed(_bus.SpeedKmh());
                _hud.SetGear(_bus.Powertrain.GearLabel);
                _hud.SetFuel(_bus.FuelTank.Level01());
                float hour = _session.DayNight.Hour;
                int hh = Mathf.FloorToInt(hour); int mm = Mathf.FloorToInt((hour - hh) * 60f);
                _hud.SetTime(hh.ToString("00") + ":" + mm.ToString("00"));
                _hud.UpdateRoute(_currentStop != null ? _currentStop.Name(AlgerBusSim.Localization.Localization.Language) : "", _currentStop != null ? Vector3.Distance(_bus.transform.position, _currentStop.BusPosition) : 0f, _onboard, _earned, _stopIndex, _layout.Stops.Count);

                // fuel low
                if (_bus.FuelTank.IsLow()) _hud.Toast(AlgerBusSim.Localization.Localization.Get("low_fuel"));

                yield return null;
            }
        }

        private float _redLightCooldown = 0f;

        private Vector3 NearestJunction()
        {
            // c5 / r5
            if (_layout.Graph.GetNode("c5") != null)
            {
                var n = _layout.Graph.GetNode("c5");
                var v = new Vector3(n.x, 0, n.z);
                if (Vector3.Distance(_bus.transform.position, v) < Vector3.Distance(_bus.transform.position, new Vector3(_layout.Graph.GetNode("r5").x,0,_layout.Graph.GetNode("r5").z)))
                    return v;
                var n2 = _layout.Graph.GetNode("r5");
                return new Vector3(n2.x, 0, n2.z);
            }
            return _bus.transform.position;
        }

        private void ServeStop()
        {
            _stopServed = true;
            _dwellTimer = 0f;

            // alighting
            int alight = UnityEngine.Random.Range(Mathf.Max(1, _onboard / 5), Mathf.Max(2, _onboard / 3 + 1));
            alight = Mathf.Min(alight, _onboard);
            _onboard -= alight;

            // boarding
            int waiting = _currentStop.Waiting.Count;
            int board = Mathf.Min(waiting, _capacity - _onboard);
            board = Mathf.Min(board, UnityEngine.Random.Range(4, 12));
            board = Mathf.Clamp(board, 0, waiting);
            _onboard += board;
            _carried += board;
            _earned += board * _econ.urbanTicketDzd;
            _earned += _econ.stopBonusDzd;

            // punctuality
            float serviceMin = _elapsedSeconds / 12f;
            float expectedMin = _route.timetable.scheduleMin[_stopIndex];
            if (Mathf.Abs(serviceMin - expectedMin) <= 3f) { _earned += _econ.punctualityBonusDzd; _punctualityHits++; }

            // hide waiting passengers
            int hidden = 0;
            foreach (var w in _currentStop.Waiting)
            {
                if (hidden >= board) break;
                if (w != null) w.SetActive(false);
                hidden++;
            }

            _hud.Toast(AlgerBusSim.Localization.Localization.Get("stop_served") + " +" + (_econ.stopBonusDzd + board * _econ.urbanTicketDzd) + " DA");
            _session.Audio.PlayBell();

            // close doors after a short delay
            StartCoroutine(CloseDoorsAfter(2.2f));
        }

        private IEnumerator CloseDoorsAfter(float t)
        {
            yield return new WaitForSeconds(t);
            _bus.DoorSystem.SetOpen(false);
            _session.Audio.PlayDoor();
        }

        private void CheckSpeeding()
        {
            float limit = SpeedLimitAt(_bus.transform.position);
            if (_bus.SpeedKmh() > limit + 12f)
            {
                if (_speedingCooldown <= 0f)
                {
                    _speedingCooldown = 6f;
                    _earned -= _econ.penalties.speedingDzd;
                    _hud.Toast(AlgerBusSim.Localization.Localization.Get("penalty_speeding") + " -" + _econ.penalties.speedingDzd);
                }
            }
            if (_speedingCooldown > 0f) _speedingCooldown -= Time.deltaTime;
        }

        private float _speedingCooldown = 0f;

        private float SpeedLimitAt(Vector3 pos)
        {
            float best = 60f;
            float bestDist = float.MaxValue;
            foreach (var e in _layout.Graph.Edges)
            {
                var pts = _layout.Graph.SampleEdge(e);
                foreach (var p in pts)
                {
                    float d = Vector3.Distance(p, pos);
                    if (d < bestDist) { bestDist = d; best = e.speedLimit; }
                }
            }
            return bestDist < 16f ? best : 60f;
        }

        private void Finish()
        {
            var summary = new Summary
            {
                Earnings = _earned,
                Passengers = _carried,
                StopsServed = _layout.Stops.Count,
                PunctualityHits = _punctualityHits,
                TotalStops = _layout.Stops.Count
            };
            _session.EndService(summary);
        }
    }
}
