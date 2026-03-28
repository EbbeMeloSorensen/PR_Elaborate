using System.Linq;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Craft.IO.Utils;
using Craft.DataStructures.Graph;
using Craft.DataStructures.IO;
using PR.Domain.Entities.PR;

namespace PR.IO
{
    public class DataIOHandler : IDataIOHandler
    {
        private XmlSerializer _xmlSerializer;

        private XmlSerializer XmlSerializer
        {
            get
            {
                if (_xmlSerializer != null)
                    return _xmlSerializer;

                var xOver = new XmlAttributeOverrides();
                var attrs = new XmlAttributes { XmlIgnore = true };
                _xmlSerializer = new XmlSerializer(typeof(PRData), xOver);

                return _xmlSerializer;
            }
        }

        public void ExportDataToXML(
            PRData prData, 
            string fileName)
        {
            using (var streamWriter = new StreamWriter(fileName))
            {
                XmlSerializer.Serialize(streamWriter, prData);
            }
        }

        public void ExportDataToJson(
            PRData prData, 
            string fileName)
        {
            var jsonResolver = new ContractResolver();
            jsonResolver.IgnoreProperty(typeof(Person), "ObjectPeople");
            jsonResolver.IgnoreProperty(typeof(Person), "SubjectPeople");

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = jsonResolver
            };

            var json = JsonConvert.SerializeObject(
                prData,
                Formatting.Indented,
                settings);

            using var streamWriter = new StreamWriter(fileName);

            streamWriter.WriteLine(json);
        }

        public void ExportDataToGraphML(
            PRData prData,
            string fileName)
        {
            var vertices = prData.People.Select(p => new LabelledVertex(p.FirstName));
            var graph = new GraphAdjacencyList<LabelledVertex, EmptyEdge>(vertices, true);

            graph.WriteToFile(fileName, Format.GraphML);
        }

        public void ImportDataFromXML(
            string fileName, 
            out PRData prData)
        {
            using var streamReader = new StreamReader(fileName);

            prData = XmlSerializer.Deserialize(streamReader) as PRData;
        }

        public void ImportDataFromJson(
            string fileName, 
            out PRData prData)
        {
            using var streamReader = new StreamReader(fileName);

            var json = streamReader.ReadToEnd();
            prData = JsonConvert.DeserializeObject<PRData>(json);
        }
    }
}
