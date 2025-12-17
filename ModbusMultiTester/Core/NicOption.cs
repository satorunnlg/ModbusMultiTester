using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ModbusMultiTester.Core
{
    /// <summary>
    /// コンボボックスに表示するNIC（ネットワークインターフェース）の選択肢
    /// </summary>
    public class NicOption
    {
        public string DisplayName { get; set; } = "";
        public IPAddress Ip { get; set; } = IPAddress.Any;

        // ComboBoxはこのToString()の結果を表示に使用します
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
