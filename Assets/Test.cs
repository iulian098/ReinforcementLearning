using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    class MyClass : GameEvent { }
    // Start is called before the first frame update
    void Start()
    {
        EventsManager.AddListener<MyClass>(OnMyClass);
    }

    private void OnMyClass(MyClass e) {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos() {
        
    }
}
