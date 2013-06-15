using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using TPulseAPI;

namespace TArena
{
    public class ArenaSetting
    {
        public APoint TeamStart1 { get; set; }
        public APoint TeamStart2 { get; set; }
        public APoint TeamPrepare1 { get; set; }
        public APoint TeamPrepare2 { get; set; }
        public APoint ArenaHall { get; set; }
        public APoint AfterMatchRoom { get; set; }
        public int PreparationTime { get; set; }
        public List<BuffType> Buffs { get; set; }
        public int PointsToWin { get; set; }

        public ArenaSetting()
        {
            PreparationTime = 15 * 1000;
            PointsToWin = 2;
            TeamStart1 = new APoint();
            TeamStart2 = new APoint();
            TeamPrepare1 = new APoint();
            TeamPrepare2 = new APoint();
            ArenaHall = new APoint();
            AfterMatchRoom = new APoint();

            Buffs = new List<BuffType>();

            //Buffs.Add(BuffType.WellFed);
            //Buffs.Add(BuffType.Ironskin);
           //Buffs.Add(BuffType.Regeneration);
            //Buffs.Add(BuffType.ManaRegeneration);

        }

        private static XmlSerializer serializer = new XmlSerializer(typeof(ArenaSetting));

        public void Save(string path)
        {
            FileStream stream = File.Create(path);
            serializer.Serialize(stream, this);
            stream.Close();
        }

        public static ArenaSetting Load(string path)
        {
            ArenaSetting asetting = new ArenaSetting();

            if(File.Exists(path))
            {

                FileStream stream = File.OpenRead(path);
                asetting = (ArenaSetting)serializer.Deserialize(stream);
                stream.Close();
            }
            else
            {
                asetting.Save(path);
            }

            return asetting;
        }
    }
}
