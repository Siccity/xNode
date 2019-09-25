using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XNode;
using XNode.Examples.LogicToy;

namespace XNodeEditor.Examples.LogicToy {
	[CustomNodeGraphEditor(typeof(LogicGraph))]
	public class LogicGraphEditor : NodeGraphEditor {
		readonly Color boolColor = new Color(0.1f, 0.6f, 0.6f);
		private List<ObjectLastOnTimer> lastOnTimers = new List<ObjectLastOnTimer>();
		private double lastFrame;

		/// <summary> Used for tracking when an arbitrary object was last 'on' for fading effects </summary>
		private class ObjectLastOnTimer {
			public object obj;
			public double lastOnTime;

			public ObjectLastOnTimer(object obj, bool on) {
				this.obj = obj;
			}
		}

		/// <summary> 
		/// Overriding GetNodeMenuName lets you control if and how nodes are categorized.
		/// In this example we are sorting out all node types that are not in the XNode.Examples namespace.
		/// </summary>
		public override string GetNodeMenuName(System.Type type) {
			if (type.Namespace == "XNode.Examples.LogicToy") {
				return base.GetNodeMenuName(type).Replace("X Node/Examples/Logic Toy/", "");
			} else return null;
		}

		public override void OnGUI() {
			// Repaint each frame
			window.Repaint();

			// Timer
			if (Event.current.type == EventType.Repaint) {
				for (int i = 0; i < target.nodes.Count; i++) {
					ITimerTick timerTick = target.nodes[i] as ITimerTick;
					if (timerTick != null) {
						float deltaTime = (float) (EditorApplication.timeSinceStartup - lastFrame);
						timerTick.Tick(deltaTime);
					}
				}
			}
			lastFrame = EditorApplication.timeSinceStartup;
		}

		/// <summary> Controls graph noodle colors </summary>
		public override Color GetNoodleColor(NodePort output, NodePort input) {
			LogicNode node = output.node as LogicNode;
			Color baseColor = base.GetNoodleColor(output, input);

			return GetLerpColor(baseColor, Color.yellow, output, (bool) node.GetValue(output));
		}

		/// <summary> Controls graph type colors </summary>
		public override Color GetTypeColor(System.Type type) {
			if (type == typeof(bool)) return boolColor;
			else return GetTypeColor(type);
		}

		/// <summary> Returns the time at which an arbitrary object was last 'on' </summary>
		public double GetLastOnTime(object obj, bool high) {
			ObjectLastOnTimer timer = lastOnTimers.FirstOrDefault(x => x.obj == obj);
			if (timer == null) {
				timer = new ObjectLastOnTimer(obj, high);
				lastOnTimers.Add(timer);
			}
			if (high) timer.lastOnTime = EditorApplication.timeSinceStartup;
			return timer.lastOnTime;
		}

		/// <summary> Returns a color based on if or when an arbitrary object was last 'on' </summary>
		public Color GetLerpColor(Color off, Color on, object obj, bool high) {
			double lastOnTime = GetLastOnTime(obj, high);

			if (high) return on;
			else {
				float t = (float) (EditorApplication.timeSinceStartup - lastOnTime);
				t *= 8f;
				if (t < 1) return Color.Lerp(on, off, t);
				else return off;
			}
		}
	}
}