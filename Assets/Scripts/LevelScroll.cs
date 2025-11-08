using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelScroll : MonoBehaviour
{
    public ScrollRect scrollRect;
    private int currentPage = 0;     // dang o page nao
    public int totalPages = 3;       // tong so page

    public void NextPage()
    {
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            ScrollToPage(currentPage);
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            ScrollToPage(currentPage);
        }
    }

    void ScrollToPage(int pageIndex)
    {
        float normalizedPos = (float)pageIndex / (totalPages - 1);
        scrollRect.horizontalNormalizedPosition = normalizedPos;
    }
}
