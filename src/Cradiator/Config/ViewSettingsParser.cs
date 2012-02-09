using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Cradiator.Config
{
    public class ViewSettingsParser
    {
        private const string ProjectRegex = "project-regex";
        private const string CategoryRegex = "category-regex";
        private const string ServerRegex = "server-regex";
        private const string Url = "url";
        private const string Skin = "skin";
        private const string ViewName = "name";
        private const string ShowOnlyBroken = "showOnlyBroken";
        private const string ShowServerName = "showServerName";
        private const string ShowOutOfDate = "showOutOfDate";
        private const string OutOfDateDifferenceInMinutes = "outOfDateDifferenceInMinutes";

        private readonly XDocument _xdoc;

        public ViewSettingsParser(TextReader xml)
        {
            _xdoc = XDocument.Parse(xml.ReadToEnd());
        }

        public static ICollection<ViewSettings> Read(string xmlFile)
        {
            using (var stream = new StreamReader(xmlFile))
            {
                var reader = new ViewSettingsParser(stream);
                return reader.ParseXml();
            }
        }

        public ICollection<ViewSettings> ParseXml()
        {
            return new ReadOnlyCollection<ViewSettings>(
                (from view in _xdoc.Elements("configuration")
                     .Elements("views")
                     .Elements("view")
                 select new ViewSettings
                            {
                                URL = view.Attribute(Url) == null ? "" : view.Attribute(Url).Value,
                                ProjectNameRegEx =
                                    view.Attribute(ProjectRegex) == null ? ".*" : view.Attribute(ProjectRegex).Value,
                                CategoryRegEx =
                                    view.Attribute(CategoryRegex) == null ? ".*" : view.Attribute(CategoryRegex).Value,
                                ServerNameRegEx =
                                    view.Attribute(ServerRegex) == null ? ".*" : view.Attribute(ServerRegex).Value,
                                SkinName = view.Attribute(Skin) == null ? "StackPhoto" : view.Attribute(Skin).Value,
                                ViewName = view.Attribute(ViewName) == null ? "" : view.Attribute(ViewName).Value,
                                ShowOnlyBroken =
                                    view.Attribute(ShowOnlyBroken) == null
                                        ? false
                                        : bool.Parse(view.Attribute(ShowOnlyBroken).Value),
                                ShowServerName =
                                    view.Attribute(ShowServerName) == null
                                        ? false
                                        : bool.Parse(view.Attribute(ShowServerName).Value),
                                ShowOutOfDate =
                                    view.Attribute(ShowOutOfDate) == null
                                        ? false
                                        : bool.Parse(view.Attribute(ShowOutOfDate).Value),
                                OutOfDateDifferenceInMinutes =
                                    view.Attribute(OutOfDateDifferenceInMinutes) == null
                                        ? 0
                                        : int.Parse(view.Attribute(OutOfDateDifferenceInMinutes).Value)
                            }).ToList());
        }

        //-----
        // modify functionality (below) is only for the settings dialog save functionality
        //-----

        public static void Modify(string xmlFile, ViewSettings viewSettings)
        {
            string xmlUpdated;
            using (var stream = new StreamReader(xmlFile))
            {
                var parser = new ViewSettingsParser(stream);
                xmlUpdated = parser.CreateUpdatedXml(viewSettings);
            }
            using (var stream = new StreamWriter(xmlFile))
            {
                stream.Write(xmlUpdated);
            }
        }

        public string CreateUpdatedXml(IViewSettings settings)
        {
            var view1 = _xdoc.Elements("configuration")
                .Elements("views")
                .Elements("view").First(); // only used to update a view when there is 1

            view1.Attribute(Url).Value = settings.URL;
            view1.Attribute(ProjectRegex).Value = settings.ProjectNameRegEx;
            view1.Attribute(CategoryRegex).Value = settings.CategoryRegEx;
            view1.Attribute(ServerRegex).Value = settings.ServerNameRegEx;
            view1.Attribute(Skin).Value = settings.SkinName;
            view1.Attribute(ViewName).Value = settings.ViewName;
            view1.Attribute(ShowOnlyBroken).Value = settings.ShowOnlyBroken.ToString();
            view1.Attribute(ShowServerName).Value = settings.ShowServerName.ToString();
            view1.Attribute(ShowOutOfDate).Value = settings.ShowOutOfDate.ToString();
            view1.Attribute(OutOfDateDifferenceInMinutes).Value = settings.OutOfDateDifferenceInMinutes.ToString();

            var xml = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(xml, new XmlWriterSettings
                                                             {
                                                                 OmitXmlDeclaration = true,
                                                                 NewLineHandling = NewLineHandling.None,
                                                                 Indent = true,
                                                             }))
            {
                _xdoc.WriteTo(xmlWriter);
            }
            return xml.ToString();
        }
    }
}