using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace SkyscraperModelStat
{
   class Program
   {
      class SkyscraperModelHistory
      {
         public int mVer;
         public int mHVer;
         public string mUser;
         public DateTime mDt;
      }

      class SkyscraperModelRes
      {
         public int mResType;
         public int mResNumber;
         public int mResRevision;
         public int mResSize;
      }

      class SkyscraperModel
      {
         public string mId;
         // List<SkyscraperModelHistory> mHistories;
         public List<SkyscraperModelRes> mResources;

         public SkyscraperModel()
         {
            mResources = new List<SkyscraperModelRes>();
         }
      }

      static List<string> test()
      {
         List<string> processInfoStrings = new List<string>();
         string queryJava_wmi = "select CommandLine from Win32_Process where Name='java.exe'";
         ManagementObjectSearcher searchJava_wmi = new ManagementObjectSearcher(queryJava_wmi);
         ManagementObjectCollection javaProcessCollection_wmi = searchJava_wmi.Get();
         if (javaProcessCollection_wmi == null)
            return processInfoStrings;
         foreach (ManagementObject javaProcess in javaProcessCollection_wmi)
         {
            processInfoStrings.Add((string)javaProcess["CommandLine"]);
         }
         return processInfoStrings;
      }

      static void Main(string[] args)
      {
         test();

         var models = readData();
         Dictionary<string, Tuple<int, int>> modelsize = new Dictionary<string, Tuple<int, int>>();
         foreach(var item in models)
         {
            int size = 0;
            int linkDataSize = 0;
            for(var restype = 0; restype < 16; ++restype)
            {
               //if (item.mId == "30850fca-f41f-4cd1-aab2-836a584f0916")
               //{
               //   Console.WriteLine("get it");
               //}
               var gg = from res in item.mResources
                  where res.mResType == restype
                  orderby res.mResRevision descending
                  group res by res.mResNumber into g
                  select g.First();

               int size2 = 0;
               foreach (var item2 in gg)
                  size2 += item2.mResSize;

               if (restype == 15)
                  linkDataSize = size2;

               size += size2;
            }

            modelsize.Add(item.mId, new Tuple<int,int>(size, linkDataSize));
         }

         const int oneM = 1024 * 1024;

         var fs = new FileStream("modelSize.csv", FileMode.Append);
         using(var sw = new StreamWriter(fs))
         {
            sw.WriteLine(string.Format("ModelGuid, ModelSize(MB), links number"));
            foreach(var item in modelsize)
               sw.WriteLine(string.Format("{0}, {1}, {2}", item.Key, item.Value.Item1/oneM,
                  item.Value.Item2 == 86 ? 0 : (item.Value.Item2-496)/338+1));
         }
      }


      static List<SkyscraperModel> readData()
      {
         var models = new List<SkyscraperModel>();

         //using (var hs = File.OpenText(@"C:\Users\hume\Desktop\MouseWithoutBorders\ProjectDb data for Missle\ModelHistories_1aa66035-d3f9-44f3-93dd-5e9107b9078e.csv"))
         // using (var rs = File.OpenText(@"C:\Users\hume\Desktop\MouseWithoutBorders\ProjectDb data for Missle\ModelResources_1aa66035-d3f9-44f3-93dd-5e9107b9078e.csv"))
         using (var rs = File.OpenText(@"C:\Users\hume\Desktop\MouseWithoutBorders\ProjectDb data for Missle\ModelResources_d293ecca-dda9-453d-99fe-c5b8d8866755.csv"))
         {
            // skip the head
            // hs.ReadLine();
            rs.ReadLine();

            var model = new SkyscraperModel();
            while(rs.Peek() >= 0)
            {
               var line = rs.ReadLine();
               var strs = line.Split(',');

               if (model.mId != strs[0])
               {
                  if(!string.IsNullOrEmpty(model.mId))
                     models.Add(model);

                  model = new SkyscraperModel();
                  model.mId = strs[0];
               }

               var res = new SkyscraperModelRes();
               res.mResType = Int32.Parse(strs[1]);
               res.mResNumber = Int32.Parse(strs[2]);
               res.mResRevision = Int32.Parse(strs[3]);
               res.mResSize = Int32.Parse(strs[8]);

               // filter the .psd files
               if (res.mResType != 13 || res.mResType != 14)
               {
                  model.mResources.Add(res);
               }
            }
         }

         return models;
      }
   }
}
