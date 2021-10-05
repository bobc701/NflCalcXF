using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NflCalc {

   static class ExtentionMethods     {
         
      public static int[] GenIndexArray(int min, int max) {
      // --------------------------------------------
      // This returns an array of integers min..max.
      // Length will be max-min+1.
      // --------------------------------------------
         var a = new int[max-min+1];
         int n = min;
         for (int i = 0; i < a.Length; i++) a[i] = n++;
         return a;
      }

      public static int[] RandomizeArray(this int[] arr, Random rn = null) {
      // ----------------------------------------------
      // This will randomize the order of the elements of arr.
      // ----------------------------------------------

      // Start from the last element and swap one by one. We don't
      // need to run for the first element that's why i > 0
         int j;
         if (rn == null) rn = new Random();
         for (int i = arr.Length - 1; i > 0; i--) {
            j = rn.Next(i+1); // j will be 0..i

         // Swap arr[i] with arr[j]...
            int m = arr[i];
            arr[i] = arr[j];
            arr[j] = m;
         }
   
         return arr;
      }


   }
   
}
