using Unity.VisualScripting;
using UnityEngine;

namespace CHM.VisualScriptingKai
{
    // Fix for coroutine from https://github.com/RealityStop/Bolt.Addons.Community/blob/master/Runtime/Events/Nodes/ManualEvent.cs
    [UnitTitle("Test")]
    [UnitCategory("Events\\VSKai")]//Set the path to find the node in the fuzzy finder as Events > My Events.
    public class TestUnit : MachineEventUnit<EmptyEventArgs>, IGraphElementWithData
    {
        public new class Data : EventUnit<EmptyEventArgs>.Data
        {
            //Tracked per instance of the node.
            public int LastObservedUpdateTicker = 0;
        }

        public override IGraphElementData CreateData()
        {
            return new Data();
        }

        protected override string hookName => EventHooks.Update;

        //Tracked class-wide (because it updates outside of the graph scope)
        [DoNotSerialize]
        public int shouldTriggerNextUpdateTicker;

        public void TriggerButton(GraphReference reference)
        {
            //By default, use immediate mode execution.  Even coroutines, which we just have to show a warning for and move on.
            bool immediate = true;

            if (Application.isEditor)
            {
                if (Application.isPlaying)
                {
                    //Coroutines always have to defer in play mode.
                    if (coroutine)
                        immediate = false;
                    else
                        immediate = false;
                }
            }

            if (immediate)
            {
                //In the editor, we just fire immediately.

                if (coroutine)
                    Debug.LogWarning("This manual event is marked as a coroutine, but Unity coroutines are only valid during Play mode.  Attempting non-coroutine activation!");
                Flow flow = Flow.New(reference);
                flow.Run(trigger);
            }
            else
            {
                shouldTriggerNextUpdateTicker++;        //When run in game, we sync it to the update
            }
        }

        protected override bool ShouldTrigger(Flow flow, EmptyEventArgs args)
        {
            var data = flow.stack.GetElementData<Data>(this);

            if (!data.isListening)
            {
                return false;
            }

            if (shouldTriggerNextUpdateTicker > data.LastObservedUpdateTicker)
            {
                data.LastObservedUpdateTicker = shouldTriggerNextUpdateTicker;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
