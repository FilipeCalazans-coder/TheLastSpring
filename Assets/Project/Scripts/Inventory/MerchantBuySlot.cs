using UnityEngine;
using UnityEngine.UI;
using TMPro; // Usando TextMeshPro para melhor visual

namespace Project.Scripts.Inventory
{
    public class MerchantBuySlot : MonoBehaviour
    {
        [Header("Referências Visuais")]
        public Image itemIcon;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI priceText;
        public TextMeshProUGUI descriptionText;

        private ItemData _item;
        private MerchantController _merchant;

        // Preenche as informações do item na linha da lista
        public void Setup(ItemData item, MerchantController merchant)
        {
            _item = item;
            _merchant = merchant;

            itemIcon.sprite = item.icon;
            nameText.text = item.itemName;
            priceText.text = $"{item.soulValue} Almas";
            descriptionText.text = item.description;
        }

        // Função chamada pelo botão "Comprar" nesta linha
        public void BuyItem()
        {
            if (_item != null && _merchant != null)
            {
                _merchant.ProcessPurchase(_item);
            }
        }
    }
}