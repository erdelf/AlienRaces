namespace AlienRace;

using System.Collections.Generic;
using System.Text;
using System.Xml;
using ExtendedGraphics;
using JetBrains.Annotations;

public partial class AlienPartGenerator
{
    public class ExtendedConditionGraphic : AbstractExtendedGraphic
    {
        public List<Condition> conditions = [];

        [UsedImplicitly]
        public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (Condition.XmlNameParseKeys.ContainsKey(xmlRoot.LocalName))
            {
                XmlDocument xmlDoc = new();

                StringBuilder xmlRaw = new("<root>");
                if (xmlRoot.Value == null)
                    xmlRaw.Append($"<path>{xmlRoot.FirstChild.Value}</path>");

                xmlRaw.Append($"<conditions><{xmlRoot.LocalName}>{xmlRoot.Attributes!["For"].Value}</{xmlRoot.LocalName}></conditions></root>");
                xmlDoc.LoadXml(xmlRaw.ToString());

                if (xmlRoot.Value != null)
                    xmlRoot.Value = string.Empty;

                foreach (XmlNode childNode in xmlDoc.DocumentElement!.ChildNodes) 
                    xmlRoot.AppendChild(xmlRoot.OwnerDocument!.ImportNode(childNode, true));
            }
            //Log.Message("Import Result: " + xmlRoot.OuterXml);

            //Log.Message("Graphic: " + xmlRoot.OuterXml);
            this.SetInstanceVariablesFromChildNodesOf(xmlRoot);
        }

        public override bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, ref ResolveData data)
        {
            ResolveData resolveData = data;

            bool applicable = this.conditions.TrueForAll(c => c.Satisfied(pawn, ref resolveData));

            if (applicable) 
                data = resolveData;

            return applicable;
        }
    }
}