using Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BotThoughts", menuName = "Config/BotThoughts")]
public class BotRoundThoughtsConfig : BaseSettings
{
    [SerializeField]
    private List<string> _thoughts;

    public List<string> Thoughts => _thoughts;  
}
