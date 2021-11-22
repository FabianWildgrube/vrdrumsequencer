using UnityEngine;

public class DefaultCustomizableEnvironment : MonoBehaviour, ICostumizableEnvironment
{
    public void activate()
    {
        //do nothing except reset the sun
        ToDLightHandler.instance.updateToD(0.5f);
    }

    public EnvironmentOptions get()
    {
        //no meaning, it's the (constant) default environment
        return EnvironmentOptions.getDefaults();
    }

    public void set(EnvironmentOptions newOptions)
    {
        //do nothing, it's the (constant) default environment
    }

    public void update(EnvironmentOptions newOptions)
    {
        //do nothing, it's the (constant) default environment
    }

    public bool daylightShouldBeActivated()
    {
        return true;
    }

    public void refreshReflections()
    {
        //do nothing
    }
}
