Marching Square Algorithm
===

![](Images/Pasted%20image%2020210520173007.png)

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

![](Images/Pasted%20image%2020210517004041.png)

## Mesh procedurali
Ci sono 2 basi su cui costruire le nostre mesh procedurali:
* Piano
* Cubi o Voxel

### Mesh Plane
I piani sono mesh che racchiudono al loro interno un array 2D di vertici.

* **Vantaggi: ** Veloce e di pratica applicazione. La mesh è già generata.
* **Svantaggi: ** Impossibilità di creare tunnel e grotte. 

![Procedural Generation 3D Terrain Unity - UnityList](https://raw.githubusercontent.com/killicolin/Procedural-Generation-3D-Terrain-Unity-/master/Ressources/noiseValue.png)

### Mesh Voxel 
I Chunks sono pezzi di mappa dove il principale blocco di costruzione sono i cubi.

![GitHub - niezbop/Voxel-Terrain-Generation: Simple experimentations of terrain  generation in Unity3D and MagicaVoxel to render voxels.](https://camo.githubusercontent.com/97379c90c96dfa6680be18a3e6bf6d1e9032a77788d15cd5d6f740b9a98a423b/687474703a2f2f692e696d6775722e636f6d2f4f72326e7338582e706e67)

## Scopo dell'algoritmo
L'algoritmo di marching square, tradotto "Quadrati marcianti", è un algoritmo per il rendering di  **isosuperfici** a partire da dei dati volumetrici, come una ***nuvola di punti***. 

Ogni punto, in base ad un algoritmo esterno avrà due stati: Attivo e disattivo. Quando attivo, dovrà contribuire, in qualche modo, alla formazione della **isosuperfice**.

Se i punti sono distribuiti con criterio (Magari con una funzione di noise 3D), è possibile ottenere un effetto simile.

![](Images/Pasted%20image%2020210517163249.png)

### Algoritmo

Il problema è, a partire da questa nuvola di punti, capire quali triangoli formare, in quale orientamento e in che punto.

L'algoritmo dovrà scorrere attraverso la griglia 3D con una cella ad 8 vertici.

Tramite l'intersecazione di questi punti, filtrati in precedenza, è possibile far capire all'algoritmo quali triangoli creare e in che punto.

La cella di scorrimento avrà una struttura simile.

![image](https://user-images.githubusercontent.com/55745404/118796189-a7d30c00-b89b-11eb-9ab7-b64a13a690c2.png)

Se per esempio 3 è un vertice occupato nella griglia, allora creeremmo un triangolo che taglia i **bordi** 2, 3 ed 11.

![image](https://user-images.githubusercontent.com/55745404/118796486-f2ed1f00-b89b-11eb-9b27-58a4735a729e.png)

Attraverso delle tabelle, è possibile ottenere quasi instantaneamente, le coppie di vertici per i triangoli da formare.

>![](https://upload.wikimedia.org/wikipedia/commons/thumb/a/a7/MarchingCubes.svg/350px-MarchingCubes.svg.png)
>
> 3D 2^8 = 256 combinazioni. 
> 2D 2^4 = 16 combinazioni.

L'algoritmo sarà veloce perché utilizza la tabella in accesso diretto. 

#### EdgeTable e TriTable
Ciò che rende difficile l'algoritmo è l'elevato numero di combinazioni di ogni vertice, e la necessità di rendere queste combinazioni cooerenti in modo che i triangoli si colleghino correttamente.

La prima parte dell'algoritmo prevede l'utilizzo di una **EdgeTable** che mappa i vertici del cubo.

Viene formato un indice a 8 bit (numero dei vertici), dove ogni bit 1 corrisponde al vertice e il numero decimale rappresentato l'indice dei triangoli nella **TriTable**.

```cs
cubeindex = 0;

// grid.val vedilo come uno degli 8 punti cardinali che sta scorrendo.
if (grid.val [0] < isolevel) cubeindex | = 1;
if (grid.val [1] < isolevel) cubeindex | = 2;
if (grid.val [2] < isolevel) cubeindex | = 4;
if (grid.val [3] < isolevel) cubeindex | = 8;
if (grid.val [4] < isolevel) cubeindex | = 16;
if (grid.val [5] < isolevel) cubeindex | = 32;
if (grid.val [6] < isolevel) cubeindex | = 64;
if (grid.val [7] < isolevel) cubeindex | = 128;
```

### Implementazione Marching Squares 
![image](https://user-images.githubusercontent.com/55745404/118907764-6a649200-b920-11eb-8d52-adbdadb3c585.png)
![image](https://user-images.githubusercontent.com/55745404/118907891-a7c91f80-b920-11eb-8ad7-cd5c445efd82.png)
Ogni chunk possiede 8x8 voxel squares, ed una mappa contiene inizialmente 4x4 chunks 
[Implementazione Marching Squares](https://github.com/Lucaffo/Procedural-World-Generation/tree/main/Procedural%20Voxel%20Terrain%20Generator/Assets/Scenes/Marching%20Squares)


