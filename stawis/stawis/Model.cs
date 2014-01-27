using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Stawis {
  public class Model {
    static Desulphurization des1 = new Desulphurization(1);
    static Desulphurization des2 = new Desulphurization(2);
    static Converter conv1 = new Converter(1);
    static Converter conv2 = new Converter(2);
    static Converter conv3 = new Converter(3);
    static Ladle chgl = new Ladle(1);

    Station[] stations = {conv1,conv2,conv3,des1,des2,chgl};
    List<Order> orderPool = new List<Order>();

    List<string> alarmList = new List<string>();

    Form1 form;

    public Model(Form1 form) {
      this.form = form;
    }

    public Station TapStation { get; set; }
    public Station ConvStation { get; set; }
    public Boolean OrderRefresh { get; set; }
    public Boolean AlarmRefresh { get; set; }
    public List<Order> OrderPool {
      get { return orderPool; }
    }
    public List<string> AlarmList {
      get { return alarmList; }
    }

    public void setParkPositionOfLadle(int ladleNo, Point parkPosition) {
      int stNo = ladleNo + 4;
      Station station = getStation(stNo);
      if (station is Ladle) {
        station.ParkPosition = parkPosition;
      }
    }

    public void orderTakeOver(string orderId, long weight) {
      Boolean orderExists = false;
      foreach (Order o in orderPool) {
        if (o.OrderId.Equals(orderId)) {
          orderExists = true;
          break;
        }
      }
      if (!orderExists) {
        Order o = new Order(orderId, weight);
        orderPool.Add(o);
        OrderRefresh = true;
      } else {
        newAlarm("order " + orderId + " already exists");
      }
    }

    public void newAlarm(string message) {
      alarmList.Add(message);
      AlarmRefresh = true;
    }

    public int getStationCount() {
      return stations.Length;
    }

    public Station getStation(int stNo) {
      if (stNo >= 0 && stNo < stations.Length) {
        return stations[stNo];
      } else {
        return null;
      }
    }

    public void takeoverAtDesulphurization(int desNo) {
      int stNo =desNo + 2;    // aus 1/2 wird 3/4
      if (stationIsValid(stNo)) {
        Station station = stations[stNo];
        if (station is Desulphurization) {
          if (station.State == States.FREE) {
            if (orderPool.Count > 0) {
              Order o = orderPool[0];   // übernimm ersten Auftrag der Liste
              orderPool.RemoveAt(0);    // lösche diesen aus der Liste
              station.Order = o;
              station.State = States.READY;
              station.Refresh = true;
              OrderRefresh = true;
            } else {
              newAlarm("Takeover at Desulphurization "+desNo+": no order there");
            }
          } else {
            newAlarm("Takeover at Desulphurization " + desNo + ": station not free");
          }
        } else {
          newAlarm("Takeover at Desulphurization " + desNo + ": no desulph.station");
        }
      }
    }

    public void desulphurizationOnOff(int desNo, int on) {
     int stNo = desNo + 2;    // aus 1/2 wird 3/4
      if (stationIsValid(stNo)) {
        Station station = stations[stNo];
        if (station is Desulphurization) {
           if (on == States.ON) {
              if (station.State == States.READY) {
                station.State = States.BUSY;
              } else {
                newAlarm("desulphurization station " + desNo + " not free");
              }
            } else if (on == States.OFF) {
              if (station.State == States.BUSY) {
                station.State = States.FINISHED;
              } else {
                newAlarm("desulphurization station "+desNo+" not in process");
              }
            }
            station.Refresh = true;
          } else {
            newAlarm("Desulphurization: no desulph.station");
          }
		  }
    }

    public void ladleMove(int ladleNo, int dx, int dy) {
      int stNo = ladleNo + 4;   // from ladleNo 1/2 make stationNo 5/6
      Station ladle = getStation(stNo);
      if (ladle is Ladle) {
        Point c = ladle.Center;
        ladle.Center = new Point(c.X + dx, c.Y + dy);
        ladle.Move = true;
        ladle.Refresh = true;
      } else {
        newAlarm("Ladle Move:  LadleNo " + ladleNo + " is wrong");
      }
    }

    public void setLadlePosition(int ladleNo, int x, int y) {
      int stNo = ladleNo + 4;   // from ladleNo 1/2 make stationNo 5/6
      Station ladle = getStation(stNo);
      if (ladle is Ladle) {
        Point c = ladle.Center;
        ladle.Center = new Point(x, y);
        ladle.Move = true;
        ladle.Refresh = true;
      } else {
        newAlarm("set Ladle Position:  LadleNo " + ladleNo + " is wrong");
      }
    }

    public void converterAvailable(int convNo, int on) {
      int stNo = convNo - 1;
      if (stationIsValid(stNo)) {
        Station station = stations[stNo];
        if (station is Converter) {
          if (on == States.ON) {
            if (station.State == States.INACTIVE) {
              station.State = States.FREE;
            } else {
              newAlarm("ConverterAvailable, Converter "+convNo
                    +": Set activ rejected, because station is not inactive");
            }
          } else {
           if (station.State == States.FREE) {
              station.State = States.INACTIVE;
            } else {
              newAlarm("ConverterAvailable, Converter "+convNo
                    +": Set-Inactiv rejected, because station is not free");
            }
           }
          station.Refresh = true;
        } else {
          newAlarm("ConverterAvailable: Converter "+convNo+" invalid");
        }

      }
    }

    public void transportStart() {
      Station des = null;
      for (int st = 0; st < stations.Length; st++) {
        Station station = stations[st];
        if (station is Desulphurization & station.State == States.FINISHED) {
          int distance = calculateDistance(station.Center, chgl.Center);
          if (distance < 150 && chgl.Center.X >= station.Center.X && chgl.Center.Y >= station.Center.Y) {
            des = station;
            Order o = des.Order;
            des.Order = null;
            chgl.Order = o;
            des.State = States.FREE;
            chgl.State = States.BUSY;
            des.Refresh = true;
            chgl.Refresh = true;
            break;
          }
        }
      }
      if (des == null) {
        newAlarm("TransportStart:  no desulph.station with an order found");
      }
    }

    public void charging(int on) {
      Station conv = null;
      for (int st = 0; st < stations.Length; st++) {
        Station station = stations[st];
        if (station is Converter) {
          if (on == States.ON && station.State == States.FREE) {
            int distance = calculateDistance(station.Center, chgl.Center);
            if (distance < 150 && chgl.Center.X <= station.Center.X && chgl.Center.Y <= station.Center.Y) {
              conv = station;
              conv.State = States.CHARGING;
              chgl.State = States.CHARGING;
              conv.Order = chgl.Order;
              conv.Refresh = true;
              chgl.Refresh = true;
              chgl.Move = true;
              break;
            }
          } else if (on == States.OFF && station.State == States.CHARGING) {
            Order o = chgl.Order;
            chgl.Order = null;
            chgl.State = States.FREE;
            Point p = chgl.Center;
            p.Y = p.Y + 100;
            chgl.Center = p;
            conv = station;
            conv.Order = o;
            conv.State = States.READY;
            conv.Refresh = true;
            chgl.Refresh = true;
            chgl.Move = true;
            break;
          }
        }
      }
      if (conv == null) {
        if (on == States.ON) {
          newAlarm("ChargingStart:  no converter near to charging ladle found");
        } else {
          newAlarm("ChargingEnd:  no converter in state CHARGING found");
        }
      }
    }

    public void converterTreatment(int convNo, int on) {
      int stNo = convNo - 1;
      if (stationIsValid(stNo)) {
        Station station = stations[stNo];
        if (station is Converter) {
          if (on == States.ON && station.State == States.READY) {
            station.State = States.BUSY;
          } else if (on == States.OFF && station.State == States.BUSY) {
            station.State = States.FINISHED;
          } else {
            newAlarm("ConverterTreatment: Converter " + convNo + " state:" + station.State + "  not compatible");
          }
          station.Refresh = true;
        } else {
          newAlarm("ConverterTreatment:  station " + stNo + " is no converter");
        }
      } else {
        newAlarm("ConverterTreatment: wrong converterNo: " + convNo);
      }
    }

    public void tapping(int convNo, int on) {
      int stNo = convNo - 1;
      if (stationIsValid(stNo)) {
        Station station = stations[stNo];
        if (station is Converter) {
          if (on == States.ON && station.State == States.FINISHED) {
            station.State = States.TAPPING;
          } else if (on == States.OFF && station.State == States.TAPPING) {
            station.State = States.FREE;
            chgl.State = States.FREE;
            chgl.Center = chgl.ParkPosition;
            station.Order = null;
          } else {
            newAlarm("Tapping: Converter " + convNo + " state:" + station.State + "  not compatible");
          }
          station.Refresh = true;
          chgl.Move = true;
        } else {
          newAlarm("Tapping:  station " + stNo + " is no converter");
        }
      } else {
        newAlarm("Tapping: wrong converterNo: " + convNo);
      }
    }

    public int calculateDistance(Point p1, Point p2) {
      double x = Math.Pow((p1.X - p2.X), 2);
      double y = Math.Pow((p1.Y - p2.Y), 2);
      int distance = (int)Math.Sqrt(x + y);
      return distance;
    }

    public Boolean stationIsValid(int stNo) {
      if (stNo >= 0 && stNo < stations.Length) {
        return true;
      } else {
        return false;
      }
    }
  }
}
