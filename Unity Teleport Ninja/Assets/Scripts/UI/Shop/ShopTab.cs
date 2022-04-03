using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopTab : MonoBehaviour
{
    public int tabIndex;
    [System.Serializable]
    public class ShopData
    {
        [HideInInspector]
        public int equipedIndex = 0;
        [HideInInspector]
        public bool[] isPurchased = new bool[30];

        public void SetEquiped(int equipedIndex)
        {
            this.equipedIndex = equipedIndex;
            this.isPurchased[equipedIndex] = true;
            Debug.Log(this.equipedIndex);
        }
        public void SetIsPurchased(bool isPurchased, int index)
        {
            this.isPurchased[index] = isPurchased;
        }


    }
    public ShopItem[] shopItemsList;

    public ShopData shopData = new ShopData();

    GameObject g;


    // Start is called before the first frame update
    void OnEnable()
    {
        GenerateShopArray();
        LoadShopData();
        SetButtonStates();
    }


    void LoadShopData()
    {
        string fileName = "shopdata" + tabIndex;
        shopData = BinarySerializer.Load<ShopData>(fileName);
    }

    void SetButtonStates()
    {
        for (int i = 0; i < shopItemsList.Length; i++)
        {
            if (shopData.isPurchased[i])
                shopItemsList[i].ChangeButtonState(ButtonState.EQUIPABLE);
        }
        shopItemsList[shopData.equipedIndex].ChangeButtonState(ButtonState.EQUIPED);
    }

    void GenerateShopArray()
    {
        shopItemsList = FindObjectsOfType<ShopItem>();

        for (int i = 1; i < shopItemsList.Length; i++)
        {
            int j = i;
            while (j > 0 && shopItemsList[j].MyIndex < shopItemsList[j - 1].MyIndex)
            {
                ShopItem temp = shopItemsList[j - 1];
                shopItemsList[j - 1] = shopItemsList[j];
                shopItemsList[j] = temp;
                j--;
            }
        }
    }

    public void ChangeEquipped(int index)
    {
        for (int i = 0; i < shopItemsList.Length; i++)
        {
            if (shopItemsList[i].IsPurchased)
                shopItemsList[i].ChangeButtonState(ButtonState.EQUIPABLE);
        }
        shopItemsList[index].ChangeButtonState(ButtonState.EQUIPED);

        //save data
        shopData.SetEquiped(index);
        BinarySerializer.Save(shopData, "shopdata" + tabIndex);
    }
}

