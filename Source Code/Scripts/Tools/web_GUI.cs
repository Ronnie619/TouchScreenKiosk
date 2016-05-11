
using UnityEngine;

public class Web_GUI : MonoBehaviour
{
    public Vector2 Position;
    public float X, Y;

    public bool HasFocus = true;

    void OnGUI()
    {       
        UWKWebView view = gameObject.GetComponent<UWKWebView>();

        if (view != null && view.Visible())
        {
            Rect r = new Rect (Position.x + X, Position.y + Y, view.CurrentWidth, view.CurrentHeight);
            view.DrawTexture (r);

            if (HasFocus)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y; 

                mousePos.x -= Position.x + X;
                mousePos.y -= Position.y + Y;    

                view.ProcessMouse(mousePos);            

                if (Event.current.isKey)
                    view.ProcessKeyboard(Event.current);
            }
        }       
    }
}