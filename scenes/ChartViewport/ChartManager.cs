using Godot;
using System;

//Lets say this inits all the initial notes and manages the chart BG.
		
//What does this do?
//Input, visual looping, timing, battle stuff, combo, note creation
		
//Focus on the looping
/*

 This should manage creating sprites for notes???
 This should manage subview camera pos and zoom.

Movement should primarily be done from a parent node
BackGround probably needs 2 sprites or parallax:
	Get a set length, based on viewport and loop/song length (Const PLAYWIDTH)
	Once one BG hits a certain left pos return it to the right pos

Notes are similar, but only need 1 representation.
	Once hits left bounds return to right bounds
	(Something else should probably manage refreshing, input, etc)
	Can probably use an object pool

If timing based input checking:
	This is enough, notes are visually just sprites
	Collision based - This might need to manage that, or have a sister manager that does, notes need more stuff on their own
 */

public partial class ChartManager : SubViewportContainer
{
	
	//Simulated variables, remove later
		private const int Bpm = 120;
	private const double SongLength = 160; //secs
	
	//Arbitrary vars, play with these
	private const double ChartLength = 1400;
	private const int NumLoops = 5; //TODO: Loops should be based on measures of a song?
	
	//Nodes from scene
	[Export] public CanvasGroup ChartLoopables;
	
	private double _loopLen; //secs
	private int _beatsPerLoop;
	
	public override void _Ready()
	{
		_loopLen = SongLength / NumLoops;
		_beatsPerLoop = (int)(_loopLen / (60f / Bpm));
		
	
	}

	public override void _Process(double delta)
	{
		//ChartLoopables.Position += 10 * Vector2.Left;
	}
}
