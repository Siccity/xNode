using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode {

[System.Serializable]
public class Theme : ScriptableObject
{
    [Header("Node Settings")]
    public Color tint = new Color (90, 97, 105, 255);
    public Color selection = new Color (255, 255, 255, 255);
    public XNodeEditor.NoodlePath noodlePath = XNodeEditor.NoodlePath.Curvy;
    public float noodleThickness = 2;
    public XNodeEditor.NoodleStroke noodleStroke = XNodeEditor.NoodleStroke.Full;
    [Tooltip("makes the dot outer switch colors with the dot, as well as it makes the dot outer infrot of the dot")]public bool makeTheDotOuterInfrontOfFill = false;
    [Header("Graph Settings")]
    public Color gridLinesColor = new Color (59, 59, 59, 255);
    public Color backgroundColor = new Color (48, 48, 48, 255);
    [Header("Node Pictures")]
    [Tooltip("an xNode dot picture that has dimensions that relates to 16x16")] public Texture2D xNodeDot;
    [Tooltip("an xNode dot outer picture that has dimensions that relates to 16x16")] public Texture2D xNodeDotOuter;
    [Tooltip("an xNode node picture that has dimensions that relates to 64x64")] public Texture2D xNodeNode;
    [Tooltip("an xNode node highlight picture that has dimensions that relates to 64x64")] public Texture2D xNodeNodeHighlight;
    [Header("Node Header Settings")]
    [Tooltip("if empty, xNode will use the defualt text")] public Font headerFont;
    public FontStyle headerFontStyle = FontStyle.Bold;
    public Color headerColor = Color.white;
    public int headerFontSize = 13;
    [Tooltip("you can adjust the padding to make the node gui content fit to the node picture")] public RectOffset padding;
}


}