using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
//using System.BitConverter;

namespace BasicNet
{
    public class NetCrypto
    {
        public static MD5 md5Hash = null;// you need create a NetCrypto object as a global variable for using static function
        
        public NetCrypto()
        {
            md5Hash = MD5.Create();
        }
        ~NetCrypto()
        {
            md5Hash.Dispose();
        }
        /// <summary>
        ///  Return a unique string of 32 characters, hash string, peresenting for a Jagged array and a string
        /// </summary>
        /// <param name="finput1">The Jagged array </param>
        /// <param name="sinput2">The string</param>
        /// <returns>Hash value</returns>
        public static string GetMd5Hash(float[][] finput1, string sinput2 )
        {

            // Convert the input string to a byte array and compute the hash.
            // Convert the input string to a byte array and compute the hash.
            byte[] binput = ConvertToByte(finput1);
            

            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            
            byte[] string2 = encoding.GetBytes(sinput2);

            binput=binput.Concat(string2).ToArray();

            byte[] data = md5Hash.ComputeHash(binput);

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        public static string GetMd5Hash(float[][] finput1, float[][] finput2)
        {

            // Convert the input string to a byte array and compute the hash.
            // Convert the input string to a byte array and compute the hash.
            byte[] binput = ConvertToByte(finput1);


            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

         

            binput = binput.Concat(ConvertToByte(finput2)).ToArray();

            byte[] data = md5Hash.ComputeHash(binput);

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        public static string GetMd5Hash(float[][] finput1,float[][] finput2, string sinput2)
        {

            // Convert the input string to a byte array and compute the hash.
            // Convert the input string to a byte array and compute the hash.
            byte[] binput = ConvertToByte(finput1);


            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            byte[] string2 = encoding.GetBytes(sinput2);

            binput = binput.Concat(string2).ToArray();

            binput = binput.Concat(ConvertToByte(finput2)).ToArray();

            byte[] data = md5Hash.ComputeHash(binput);

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        private static byte[] ConvertToByte(float[][] finput)
        {
            byte[] binput = null;

            //Copy finput to binput
            byte[] buffer = null;
            

            for (int i = 0; i < finput.Length; i++)
            {
                buffer = new byte[finput[i].Length * sizeof(float)];
                int t = 0;
                for (int j = 0; j < finput[i].Length; j++)
                {

                    BitConverter.GetBytes(finput[i][j]).CopyTo(buffer, t);
                    t += sizeof(float);
                }
                if (binput == null)
                    binput = buffer;
                else
                    binput = binput.Concat(buffer).ToArray();
                
            }
            return binput;
        }
        /// <summary>
        /// Return a unique string of 32 characters, hash string, peresenting for a Jagged array
        /// </summary>
        /// <param name="finput">The jagged array</param>
        /// <returns>The hash string</returns>
        public static string GetMd5Hash(float[][] finput)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] binput = ConvertToByte(finput);
            byte[] data = md5Hash.ComputeHash(binput);

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        /// <summary>
        /// Return a unique string of 32 characters, hash string, peresenting for a string
        /// </summary>
        /// <param name="finput">The string</param>
        /// <returns>The hash string</returns>
        public static string GetMd5Hash(string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        // Verify a hash against a string.
        static bool VerifyMd5Hash(string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
