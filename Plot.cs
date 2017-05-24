using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using monopoly.Annotations;

namespace monopoly
{
   
    public class Plot
    {
        public int koht;
        public string nimi;
        public string täisnimi;
        private string stops;
        public int Stops
        {
            get
            {
                return Int32.Parse(stops);
            }

            set
            {
                if (value.ToString() != stops)
                {
                stops = value.ToString();
                }
            }
        }

        public Border gElement;
    }
    
}
