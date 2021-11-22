public interface ICostumizableEnvironment
{
    /// Sets the options and triggers a relayout of all environment properties
    void set(EnvironmentOptions newOptions);

    EnvironmentOptions get();

    /// Triggers a relayout of the environment properties that have changed
    void update(EnvironmentOptions newOptions);

    void activate();

    bool daylightShouldBeActivated();

    void refreshReflections();
}
