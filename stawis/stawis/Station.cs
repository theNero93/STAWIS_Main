using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Stawis {
  public class Station {

    public Station(string type, int number) {
      Type = type;
      Number = number;
    }

    public Point Position { get; set; }
    public Point Center { get; set; }
    public string Type {get; set; }
    public int State { get; set; }
    public int Number { get; set; }
    public Order Order { get; set; }
    public Panel PanelName { get; set; }
    public Boolean Refresh { get; set; }
    public Boolean Move { get; set; }
    public Point ParkPosition { get; set; }


  }

}
