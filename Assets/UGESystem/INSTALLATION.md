# UGESystem - Installation Guide

This guide details the required setup steps after importing UGESystem into a **new Unity Project**. Please follow these instructions carefully to ensure the system functions correctly.

---

### **1. Install `Newtonsoft.Json` Package**

The UGESystem uses Newtonsoft.Json for data processing. You must install it via the Package Manager.

1.  In the Unity Editor, go to the top menu and select **`Window > Package Manager`**.
2.  In the Package Manager window that opens, click the **`+`** icon in the top-left corner.
3.  From the dropdown menu, select **`Add package by name...`**.
4.  A small window will appear. In the text field, enter the following package name exactly:
    ```
    com.unity.nuget.newtonsoft-json
    ```
5.  Click the **`Add`** button. Unity will download and install the package.

---

### **2. Install `Cinemachine` Package**

The UGESystem uses Cinemachine for cinematic camera control.

1.  In the **`Package Manager`** window, ensure the dropdown menu at the top is set to **`Packages: Unity Registry`**. This will show all official Unity packages.
2.  In the search bar at the top-right of the window, type **`Cinemachine`**.
3.  The "Cinemachine" package will appear in the list. Select it.
4.  In the bottom-right corner of the window, click the **`Install`** button.

---

### **3. Import `TextMeshPro` Essential Resources**

The UI elements in UGESystem rely on TextMeshPro.

1.  After importing the UGESystem package, a window titled **"TMP Importer"** may appear automatically.
2.  If it appears, click the **`Import TMP Essentials`** button.
3.  If the window does not appear automatically, you can open it manually by going to the top menu and selecting **`Window > TextMeshPro > Import TMP Essential Resources`**. Click the import button when the window appears.

---

### **4. Add `Character3D` Layer**

The system uses a dedicated layer for rendering 3D characters.

1.  In the Unity Editor, go to the top menu and select **`Edit > Project Settings...`**.
2.  In the Project Settings window, select **`Tags and Layers`** from the left-hand menu.
3.  In the Inspector view, find the **`Layers`** section.
4.  Find the first available empty slot in the list (e.g., `User Layer 8`).
5.  Click on the empty text field and type the following name exactly:
    ```
    Character3D
    ```
6.  Press Enter. The layer is now added. You can close the Project Settings window.

---

After completing these four steps, all dependencies will be correctly configured.