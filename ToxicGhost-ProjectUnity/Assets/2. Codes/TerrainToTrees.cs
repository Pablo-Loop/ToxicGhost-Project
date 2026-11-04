using UnityEngine;

[ExecuteInEditMode]
public class TerrainToTrees : MonoBehaviour
{
    [ContextMenu("Convertir Árboles a GameObjects")]
    public void ConvertirArboles()
    {
        Terrain terrain = GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("Este script debe estar en el objeto Terrain.");
            return;
        }

        TerrainData data = terrain.terrainData;
        GameObject contenedor = new GameObject("Arboles_Convertidos");
        contenedor.transform.position = terrain.transform.position;

        for (int i = 0; i < data.treeInstances.Length; i++)
        {
            TreeInstance tree = data.treeInstances[i];
            Vector3 posWorld = Vector3.Scale(tree.position, data.size) + terrain.transform.position;
            GameObject treePrefab = data.treePrototypes[tree.prototypeIndex].prefab;

            GameObject nuevoArbol = Instantiate(treePrefab, posWorld, Quaternion.Euler(0, tree.rotation * Mathf.Rad2Deg, 0), contenedor.transform);
            Vector3 escalaOriginal = treePrefab.transform.localScale;
            nuevoArbol.transform.localScale = new Vector3(
                escalaOriginal.x * tree.widthScale,
                escalaOriginal.y * tree.heightScale,
                escalaOriginal.z * tree.widthScale
            );
        }

        // Desactivar el dibujado de árboles del Terrain para que no se dupliquen visualmente
        terrain.drawTreesAndFoliage = false;
        Debug.Log("¡Conversión completada exitosamente!");
    }
}
