using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dm;
using tl3d.Dutility;
using System.Windows.Forms;

namespace tl3d.Dddm
{
    public  class ddm
    {
        public static int mRegFlag = 0;

        public  dmsoft sDm = null;
        public  dmsoft getinstance()
        {
            if (mRegFlag == 0)
            {
                string ret = utility.AutoRegCom("regsvr32 -s C:\\Users\\dlh\\Documents\\Visual Studio 2015\\Projects\\tl3d\\tl3d\\bin\\Debug\\dm.dll");
                mRegFlag = 1;
            }
            if (sDm == null)
            {
                    sDm = new dmsoft();
                int ret = sDm.Reg("xingming26778e0c965e7aea1a35a08a1b7981cf", "");
                if (ret != 1)
                {
                    MessageBox.Show("dm reg faile !! "+ret);
                }
            }
            return sDm;
        }
    }
}
