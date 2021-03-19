using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Trail
{
    /// <summary>
    /// UUID is used to store different IDs for multiple systems in Trail such as AuthKit.GetGameUserID or PaymentKit.GetProductPrice.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class UUID
    {
        /// <summary>
        /// The stored UUID binary length.
        /// </summary>
        public const int BytesLength = 16;

        /// <summary>
        /// The stored UUID in bytes.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = BytesLength)]
        public readonly byte[] Bytes = new byte[BytesLength];

        /// <summary>
        /// Returns whether it is a valid UUID, returns false if UUID is empty or zero.
        /// </summary>
        public unsafe bool IsValid
        {
            get
            {
                fixed (byte* ptr = (&Bytes[0]))
                {
                    int* iPtr = (int*)ptr;
                    int result =  // Add whole byte array together as an int using or-bit operation to check if all bytes are 0
                        (*(iPtr + 0)) |
                        (*(iPtr + 1)) |
                        (*(iPtr + 2)) |
                        (*(iPtr + 3));
                    return result != 0;
                }
            }
        }

        /// <summary>
        /// Returns a UUID from a string.
        /// </summary>
        /// <param name="str">The string to parse to UUID.</param>
        /// <returns>Returns a UUID if succeeded or null if it failed to convert.</returns>
        public static UUID FromString(string str)
        {
            if (str.Length != 36) { return null; }

            var id = new UUID();

            if (byte.TryParse(str.Substring(0, 2), NumberStyles.HexNumber, null, out id.Bytes[0]) &&
                byte.TryParse(str.Substring(2, 2), NumberStyles.HexNumber, null, out id.Bytes[1]) &&
                byte.TryParse(str.Substring(4, 2), NumberStyles.HexNumber, null, out id.Bytes[2]) &&
                byte.TryParse(str.Substring(6, 2), NumberStyles.HexNumber, null, out id.Bytes[3]) &&

                byte.TryParse(str.Substring(9, 2), NumberStyles.HexNumber, null, out id.Bytes[4]) &&
                byte.TryParse(str.Substring(11, 2), NumberStyles.HexNumber, null, out id.Bytes[5]) &&

                byte.TryParse(str.Substring(14, 2), NumberStyles.HexNumber, null, out id.Bytes[6]) &&
                byte.TryParse(str.Substring(16, 2), NumberStyles.HexNumber, null, out id.Bytes[7]) &&

                byte.TryParse(str.Substring(19, 2), NumberStyles.HexNumber, null, out id.Bytes[8]) &&
                byte.TryParse(str.Substring(21, 2), NumberStyles.HexNumber, null, out id.Bytes[9]) &&

                byte.TryParse(str.Substring(24, 2), NumberStyles.HexNumber, null, out id.Bytes[10]) &&
                byte.TryParse(str.Substring(26, 2), NumberStyles.HexNumber, null, out id.Bytes[11]) &&
                byte.TryParse(str.Substring(28, 2), NumberStyles.HexNumber, null, out id.Bytes[12]) &&
                byte.TryParse(str.Substring(30, 2), NumberStyles.HexNumber, null, out id.Bytes[13]) &&
                byte.TryParse(str.Substring(32, 2), NumberStyles.HexNumber, null, out id.Bytes[14]) &&
                byte.TryParse(str.Substring(34, 2), NumberStyles.HexNumber, null, out id.Bytes[15]))
            {
                return id;
            }
            return null;
        }

        /// <summary>
        /// Converts the UUID to string.
        /// </summary>
        /// <returns>The UUID as formated string.</returns>
        public override string ToString()
        {
            return string.Format(
                "{0:x2}{1:x2}{2:x2}{3:x2}-{4:x2}{5:x2}-{6:x2}{7:x2}-{8:x2}{9:x2}-{10:x2}{11:x2}{12:x2}{13:x2}{14:x2}{15:x2}",
                this.Bytes[0],
                this.Bytes[1],
                this.Bytes[2],
                this.Bytes[3],
                this.Bytes[4],
                this.Bytes[5],
                this.Bytes[6],
                this.Bytes[7],
                this.Bytes[8],
                this.Bytes[9],
                this.Bytes[10],
                this.Bytes[11],
                this.Bytes[12],
                this.Bytes[13],
                this.Bytes[14],
                this.Bytes[15]
            );
        }
    }
}
