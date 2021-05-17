Procedural World Generation
===
## Triangoli e Vertici
I **triangoli** sono una forma base usata nella grafica 3D. 
Questo principalmente per 2 motivi:
1) E' il **numero minimo di vertici** necessari per creare una **forma**.
2) E' **facilmente renderizzabile**, proprio perché non importa dove tu posizioni questi vertici, creerà sempre una ***superfice piatta***. Già da 4 punti la posizione dei punti potrebbre creare delle curve (Immagina un tacos)

## Cos'è una mesh
Una **mesh** è un'insieme di ***strutture dati***, che gestiscono come questa debba essere renderizzata in uno spazio 3D.

Una mesh contine due array importantissimi. Un array dedicato ai triangoli e un'altro dedicato ai 3 vertici di ogni triangolo. 

Ogni vertice può contenere dati sulla posizione, sulla normale, normale, sui colori, sulle tangenti e tante altre informazioni.

Principalmente nel codice troveremo una struttura come:

`Vector3[] vertices` dice alla mesh dove risiedono i punti.
`int[] triangles` dice alla mesh l'ordine con il quale fare i triangoli. L'array di int è monodimensionale proprio perché i punti verranno presi ogni 3.

![[Pasted image 20210517004041.png]]


## Mesh procedurali comuni
Ci sono 3 basi su cui costruire le nostre mesh procedurali:
* Griglie
* Cubi o Voxel

### Griglie
Le griglie sono array 2D di vertici, i quali formano un piano di celle se uniti. Esistono 2 tipi di griglie principalmente: Quadrati e  Esagoni. 

Una griglia deve sapere:
* Posizione e dimensione della singola cella.
* Numero di celle, colonne e righe.

![Procedural Generation 3D Terrain Unity - UnityList](https://raw.githubusercontent.com/killicolin/Procedural-Generation-3D-Terrain-Unity-/master/Ressources/noiseValue.png)

### Voxels
I Voxels sono mappe dove il principale blocco di costruzione sono i cubi. Questo non significa che il mondo sarà cubettoso come minecraft, significa che la struttura matematica usata potrà essere composta anche da un cubo di 3 dimensioni (Un chunk) dove al suo interno è descritta in che modo è composto il mondo.

E' uno dei metodi preferito da tutti, perché permette una maggiore flessibilità dei terreni, avendo la possibilità di creare tunnels.

![GitHub - niezbop/Voxel-Terrain-Generation: Simple experimentations of terrain  generation in Unity3D and MagicaVoxel to render voxels.](https://camo.githubusercontent.com/97379c90c96dfa6680be18a3e6bf6d1e9032a77788d15cd5d6f740b9a98a423b/687474703a2f2f692e696d6775722e636f6d2f4f72326e7338582e706e67)

## Noise Theory
Le funzioni di noise sono molto usate nella computer graphics, in quanto usano poca memoria e permettono di creare scenari complessi e dettagliati in maniera autonoma e consistente. 

Non si sta parlando di numeri pseudo-casuali.

## Uber Noises



# Unity
 ## Componenti richiesti
** Mesh Filter. ** contiene le informazioni sulla mesh.
** Mesh Render. ** renderizza la mesh.

### Costruire una mesh su unity
Quando si crea una mesh su unity, ci sono dei passaggi che vanno eseguiti nel solito ordine:
1) Assegnare i vertici
2) Assegnare i triangoli

```f
mesh.vertices = newVertices;
mesh.uv = newUV;
mesh.triangles = newTriangles;
```

### Modificare una mesh su unity
Quando si modifica, bisogna eseguiti dei passaggi con ordine:
1) Prendere i vertici
2) Modificarli
3) Riassegnarli

```f
Mesh mesh = GetComponent<MeshFilter>().mesh;
Vector3[] vertices = mesh.vertices;
Vector3[] normals = mesh.normals;

for (var i = 0; i < vertices.Length; i++)
{
	vertices[i] += normals[i] * Mathf.Sin(Time.time);
}

mesh.vertices = vertices;
```

### Aggiungere o rimuovere vertici
Quando si aggiunge o rimuove vertici da una mesh, è necessario chiamare prima di tutto il metodo `mesh.Clear()` per assicurarti di non andare out of bounds e avere errori spiacevoli sugli array.

### Normali
![[Pasted image 20210517012703.png]]

### Colori
![[Pasted image 20210517012747.png]]
![[Pasted image 20210517012856.png]]
Meglio usare come byte per evitare conversioni aggiuntive.

### UV 
![[Pasted image 20210517012925.png]]
![[Pasted image 20210517012934.png]]

## Impostare i vertici è la parte più difficile
![[Pasted image 20210517013053.png]]
Un esempio semplice, figurati se fosse stata una forma più complessa. E' difficile concettualizzare dove partire con gli indici dei vertici e dei triangoli.

### Mesh con numero di vertici e triangoli dinamici

![[Pasted image 20210517013947.png]]