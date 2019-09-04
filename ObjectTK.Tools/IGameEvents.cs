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
        event EventHandler<FrameEventArgs> UpdateFrame;

        event EventHandler<MouseMoveEventArgs> MouseMove;

        event EventHandler<MouseWheelEventArgs> MouseWheel;
    }
}
