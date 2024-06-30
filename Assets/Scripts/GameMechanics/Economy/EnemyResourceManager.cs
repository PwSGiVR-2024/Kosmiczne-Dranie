public class EnemyResourceManager : ResourceManager
{
    public int crystalsValue = 0;
    public int metalsValue = 0;
    public int creditsValue = 0;

    public override void OnResourceCapture(ResourceHolder resource, Affiliation capturer)
    {
        if (currentResources.Contains(resource) && capturer != Affiliation.Red)
        {
            currentResources.Remove(resource);

            switch (resource.resourceType)
            {
                case ResourceHolder.ResourceType.Crystals:
                    crystalsValue -= resource.value;
                    break;

                case ResourceHolder.ResourceType.Metals:
                    metalsValue -= resource.value;
                    break;

                case ResourceHolder.ResourceType.Credits:
                    creditsValue -= resource.value;
                    break;
            }
        }

        else if (capturer == Affiliation.Red)
        {
            currentResources.Add(resource);

            switch (resource.resourceType)
            {
                case ResourceHolder.ResourceType.Crystals:
                    crystalsValue += resource.value;
                    break;

                case ResourceHolder.ResourceType.Metals:
                    metalsValue += resource.value;
                    break;

                case ResourceHolder.ResourceType.Credits:
                    creditsValue += resource.value;
                    break;
            }
        }
    }
}
