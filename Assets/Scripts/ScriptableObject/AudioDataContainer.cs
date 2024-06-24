using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioDataContainer", menuName = "ScriptableObjects/Audio Data Container")]
public class AudioDataContainer : ScriptableObject
{
    [SerializeField] AudioClip buttonClickClip;
    [SerializeField] AudioClip purchaseClip;
    [SerializeField] AudioClip equipClip;

    public AudioClip ButtonClickClip => buttonClickClip;
    public AudioClip PurchaseClip => purchaseClip;
    public AudioClip EquipClip => equipClip;
}
