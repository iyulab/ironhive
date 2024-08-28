namespace Raggle.Console;

public static class Constants
{
    // Setup
    public const string SETTING_DIRECTORY = ".raggle";
    public const string SETTING_FILENAME = "setting.json";
    public const string VECTOR_DIRECTORY = "vectors";
    public const string FILES_DIRECTORY = "files";

    // Chat
    public const string WELCOME_MESSAGE = "Hello, how can I help?";
    public const string BOT_COLOR = "green";
    public const string BOT_NAME = "Bot";
    public const string USER_COLOR = "blue";
    public const string USER_NAME = "You";

    // Chat Commands
    public const string EXIT_COMMAND = "exit";
    public const string CLEAR_COMMAND = "clear";

    // Prompt
    public const string DEFAULT_SYSTEM_PROMPT =
"""
You are a helpful assistant replying to user questions using information from your memory.
Reply very briefly and concisely, get to the point immediately. Don't provide long explanations unless necessary.
Sometimes you don't have relevant memories so you reply saying you don't know, don't have the information.
""";

}
