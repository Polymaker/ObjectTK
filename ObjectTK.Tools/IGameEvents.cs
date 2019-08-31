using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectTK.Tools
{
    public interface IGameEvents
    {
        EventHandler<FrameEventArgs> UpdateFrame { get; set; }

        EventHandler<MouseMoveEventArgs> MouseMove { get; set; }

        EventHandler<MouseWheelEventArgs> MouseWheel { get; set; }
    }
}
