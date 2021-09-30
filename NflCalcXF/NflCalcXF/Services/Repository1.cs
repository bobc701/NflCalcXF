using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace NflCalcXF.Services
{
   static class Repository1
   {
      public static StreamReader GetTextFileOnDisk(string token)
      {
         var assembly = typeof(MainPage).GetTypeInfo().Assembly;

         string path = "";
         switch (token)
         {
            case "help": path = @"NflCalcXF.Resources.Help1.txt"; break;
            case "about": path = @"NflCalcXF.Resources.About1.txt"; break;
         }

         Stream strm = assembly.GetManifestResourceStream(path);
         return new StreamReader(strm);

      }



   }
}
