namespace UGESystem
{
    /// <summary>
    /// Defines the <see cref="CommandType"/> enumeration,
    /// providing a comprehensive list of all available command types within the event system.
    /// </summary>
    public enum CommandType
    {
        /// <summary>Dialogue output</summary>
        Dialogue,       
        /// <summary>Character appearance/exit/expression change</summary>
        Character,      
        /// <summary>Background change</summary>
        Background,     
        /// <summary>Display choices</summary>
        Choice,         
        /// <summary>Internal branch point (GOTO destination)</summary>
        Label,          
        /// <summary>Go to label</summary>
        Goto,           
        /// <summary>Play sound effect or BGM</summary>
        PlaySound,      
        /// <summary>End event immediately</summary>
        End,            
        /// <summary>Camera control (Cinemachine)</summary>
        Camera,         
        /// <summary>Screen effect (fade, tint, etc.)</summary>
        ScreenEffect,   
        /// <summary>Execute other GameEvent (for 2-stage graph function)</summary>
        TriggerEvent    
    }
}