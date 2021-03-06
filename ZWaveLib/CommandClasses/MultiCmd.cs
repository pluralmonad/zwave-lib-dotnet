﻿/*
    This file is part of ZWaveLib Project source code.

    ZWaveLib is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ZWaveLib is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with ZWaveLib.  If not, see <http://www.gnu.org/licenses/>.  
*/

/*
*     Author: https://github.com/mdave
*     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
*/

using System;
using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses
{
    public class MultiCmd : ICommandClass
    {
        public CommandClass GetClassId()
        {
            return CommandClass.MultiCmd;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            byte i, offset = 3;

            NodeEvent parent = null, child = null;

            Utility.logger.Debug(String.Format("MultiCmd encapsulated message: {0}", BitConverter.ToString(message)));

            if (message[1] != (byte)1)
            {
                return parent;
            }

            // Loop over each message and process it in turn.
            for (i = 0; i < message[2]; i++)
            {
                // Length and command classes of sub-command.
                byte length = message[offset];
                byte cmdClass = message[offset + 1];

                // Copy message into new array.
                var instanceMessage = new byte[length];
                Array.Copy(message, offset + 1, instanceMessage, 0, length);
                Utility.logger.Debug(String.Format("Processing message chunk: {0}", BitConverter.ToString(instanceMessage)));

                // Move offset to the next encap message
                offset += (byte)(length + 1);

                // Grab command class from the factory. If we don't have one, print out a warning and continue.
                var cc = CommandClassFactory.GetCommandClass(cmdClass);
                if (cc == null)
                {
                    Utility.logger.Warn(String.Format("Can't find CommandClass handler for command class {0}", cmdClass));
                    continue;
                }

                // Chain this event onto previously seen events.
                NodeEvent tmp = cc.GetEvent(node, instanceMessage);
                if (tmp == null)
                {
                    continue;
                }

                if (parent == null)
                {
                    parent = child = tmp;
                }
                else
                {
                    child.NestedEvent = tmp;
                    child = tmp;
                }
            }

            return parent;
        }
    }
}
