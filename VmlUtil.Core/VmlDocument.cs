namespace VmlUtil.Core
{
    public class VmlDocument : VmlObject
    {
        public VmlElement Root { get; set; }

        public override string ToXml()
        {
            return Root.ToXml();
        }
    }
}
