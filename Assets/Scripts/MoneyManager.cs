using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public void PayForPlayer(MoneyArea moneyArea)
    {
        // Player에게 돈을 지불하도록 코드 짜기 (5 ~ 15원 랜덤하게)
        int money = Random.Range(5, 16);

        moneyArea.BuildMoneyStack(money);
    }

}
