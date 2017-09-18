using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseNode : Node {

    protected override void Init() {
        inputs = new NodePort[2];
        inputs[0] = CreateNodeInput("IntInput", typeof(int));
        inputs[1] = CreateNodeInput("StringInput", typeof(string));
        outputs = new NodePort[1];
        outputs[0] = CreateNodeOutput("StringOutput", typeof(string));
    }
}
