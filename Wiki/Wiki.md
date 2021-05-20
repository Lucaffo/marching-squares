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

![](Images/Pasted%20image%2020210517004041.png)

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

# Noise Theory
Le funzioni di noise sono molto usate nella computer graphics, in quanto usano poca memoria e permettono di creare scenari complessi e dettagliati in maniera autonoma e consistente. 

E' spesso usato, per creare ancora più realismo e dare l'impressione che un pattern si ripeta, di moltiplicare noise diversi. 

## Perlin Noise
E' un tipo di noise, descritto dall'informatico **Ken Perlin**. Permette la creazione di texture procedurali omogenee.

![](Images/Pasted%20image%2020210517123243.png)

Questo oltre a permettere la creazione di texture usando pochissima memoria, permette di creare dei ***pattern*** che possono essere usati in ogni campo della generazione procedurale: dalle ***texture***, ai ***terreni***, ***mappe***, ***vegetazione***, ***nuvole*** etc..

Il fatto che sia un'algoritmo permette di essere facilmente controllabile e modificabile, permettendo di creare noise di *dimensioni* e *densità* variabile.

> Minecraft è stato realizzato utilizzando un filtro trilineare di perlin noise a bassa densità. 

> Un filtro trilineare è l'immagine creata 3 volte, che permette di essere approssimata ad una sola immagine con risoluzione in media meno spigolosa.

Il problema del perlin noise, è che il risultato spesso non porta a risultati veramente "realistici". 

![](Images/Pasted%20image%2020210517122319.png)

Dall'immagine si vede, come questo perlin in realtà, produce scenari molto ripetuti, sebbene quasi realistici. 
Non esistono dirupi e tante altre cose. 

## DEM Data
E' possibile, addestrare un intelligenza artificiale per poter ricreare questi scenari "reali". I dati in ingresso dell'algoritmo, in questo caso, vengono dati da questa AI creando un risultato come il seguente.

![](Images/Pasted%20image%2020210517122742.png)

#### Problema
Il mondo reale è noioso. Se tu potessi camminare in questa distesa infinita, ti annoieresti in subito. Mancano dettagli e surrealità. Preferiresti esplorare la pianura padana o un terreno di grotte, colline etc... ti sei dato da solo la risposta.

# Uber Noises
Aggiungiamo un layer più astratto. Cerchiamo di trovare un metodo unificato per la generazione di noise.  

### Brownian Motion 
Un movimento browniano è il movimento dove la posizione di un oggetto è dato nel tempo tramite una formula matematica. 

Questa formula definisce percorsi casuali, ma simili tra di loro, se versione zoommata di questo percorso assomiglierebbe all'intero percorso. 
(Proprio come un frattale)

#### Fractional Brownian Motion
Un movimento browniano frazionario è lo stesso procedimento ma applicato a blocchetti, dove esiste una sorta di memoria del processo. Lo stato di partenza dell'algoritmo è lo stato realizzato precedentemente; Se la memoria ha un dato positivo questo permette di ottenere risultati simili futuri sempre nella stessa direzione, creando percorsi più fluidi, altrimenti se il risultato è negativo, creera percorsi più casuali.

Questo parametro che controlla il comportamento della memoria, è chiamato esponente di Hurst, abbreviato anche in ***H***.

H assume valori da 0 a 1, decrivendo FBM ruvidi e regolari.

![](https://www.iquilezles.org/www/articles/fbm/gfx09.gif)

```cs
float fbm(Vector3 vertex, float H)
{
	float G = -H * -H;
	float f = 1.0f;
	float a = 1.0f;
	float t = 0.0f;
	
	for(int i = 0; i < numOctaves; i++)
	{
		t += a*noise(f*x);
		f *= 2.0f;
		a *= G;
	}
	return t;
}
```

![](Images/Pasted%20image%2020210517123745.png)

### Billow
Billow è essenzialmente la stessa cosa del simplex, solo che ogni valore della wave è in valore assoluto.

Permette di creare colline rotonde o ripide pieghe. 

![](Images/Pasted%20image%2020210517124219.png)

```cs
float n = Mathf.Abs(noise(freq * i, seed));
```

### Ridged
Ridged è il complemento negativo del Billow.

Permette di burroni e ripide increspature. Permette di creare delle pieghe alla base molto più rotonde.

![](Images/Pasted%20image%2020210517124514.png)

```cs
float n = 0f - Mathf.Abs(noise(freq * i, seed));
```

###  Warping o Domain Distortion
E' una tecnica comune nella generazione prcoedurale di texture e geometrie. 
Warping significa che distorciamo il dominio di una funzione con un altra. Se abbiamo una funzione `f(p)` la sua warping sarebbe una cosa del tipo `f(g(p))` etc..

Più fitri di dominio inseriamo è più realistico e definito sarà il noise ottenuto. Esempi usando Simplex:

![](Images/Pasted%20image%2020210517130603.png)
![](Images/Pasted%20image%2020210517130612.png)
![](Images/Pasted%20image%2020210517130624.png)

Le immagini sono molto esplicative, è già possibile identificare rive, montagne zone deserte etc..

Tramite questo domain warping è possibile generare algoritmi combinati per definire tutti i dettagli della mappa e controllarli bene.

# Uber Noises with Domain warping

### Sharpness
E' possibile utilizzare il **billow** e il **ridged** e combinarli assieme tramite il **warping**.

```cs
float billowNoise = Mathf.Abs(Perlin(freq * i, seed));

float ridgedNoise = 1.0f - billowNoise;

resultNoise = Mathf.Lerp(resultNoise, billowNoise)
```

## Marching Cubes Algorithm
L'algoritmo di marching cubes, "Cubi marcianti", è un calcolo che permette di creare una rappresentazione grafica di una **isosuperficie** in un campo scalare 3D. 

### Isosuperfici
Una **isosuperficie** è una superficie **3D** che rappresenta la _distribuzione di punti in uno spazio 3D_. E' l'equivalente 3D della _linea di contorno_. 

Una volta distributi questi punti in uno spazio 3D, ad essi viene associato un _valore_, tramite una funzione **`f(x,y,z) = value`**. Esiste un soglia filtro, che permette di eliminare dei vertici dal calcolo della isosuperfice. 

Se i punti sono distribuiti con criterio (Magari con una funzione di noise 3D), è possibile ottenere un effetto simile.

![image](https://user-images.githubusercontent.com/55745404/118791662-36915a00-b897-11eb-9a71-aea180486bf7.png)

### Applicazioni
* Ricostruzione di scansioni mediche, a partire da un volume di dati ottenuto da un apparecchiatura medica.

### Algoritmo
L'algoritmo è veloce perché utilizza una tabella di punti, permettendo una accesso quasi diretto a combinazioni di triangoli da realizzare. 

Il problema da risolvere è formare un'approssimazione di faccie di una isosuperficie attraverso un campo scalare campionato su una griglia 3D. 

L'algoritmo dovrà scorrere attraverso la griglia 3D con una cella ad 8 vertici. Tramite l'intersecazione di questi punti, filtrati in precedenza, è possibile far capire all'algoritmo quali triangoli creare e in che punto.

La cella di scorrimento avrà una struttura simile.

![image](https://user-images.githubusercontent.com/55745404/118796189-a7d30c00-b89b-11eb-9ab7-b64a13a690c2.png)

Se per esempio 3 è un vertice occupato nella griglia, allora creeremmo un triangolo che taglia i **bordi** 2, 3 ed 11.

![image](https://user-images.githubusercontent.com/55745404/118796486-f2ed1f00-b89b-11eb-9b27-58a4735a729e.png)

Una delle parti più importanti dell'algoritmo, dunque, è capire in che modo possa ricavare queste coppie in maniera efficente. 

#### EdgeTable e TriTable
Ciò che rende difficile l'algoritmo è l'elevato numero di combinazioni di ogni vertice `2^8 = 256` e la necessità di rendere queste combinazioni cooerenti in modo che i triangoli si colleghino correttamente.

La prima parte dell'algoritmo prevede l'utilizzo di una **EdgeTable** che mappa i vertici centrali ad ogni bordo.

Viene formato un indice a 8 bit (numero dei vertici), dove ogni bit 1 corrisponde al vertice e il numero decimale rappresentato l'indice dei triangoli nella **TriTable**.

```cs
cubeindex = 0;

if (grid.val [0] <isolevel) cubeindex | = 1;
if (grid.val [1] <isolevel) cubeindex | = 2;
if (grid.val [2] <isolevel) cubeindex | = 4;
if (grid.val [3] <isolevel) cubeindex | = 8;
if (grid.val [4] <isolevel) cubeindex | = 16;
if (grid.val [5] <isolevel) cubeindex | = 32;
if (grid.val [6] <isolevel) cubeindex | = 64;
if (grid.val [7] <isolevel) cubeindex | = 128;
```
### Implementazione Marching Squares 
![image](https://user-images.githubusercontent.com/55745404/118907764-6a649200-b920-11eb-8d52-adbdadb3c585.png)
![image](https://user-images.githubusercontent.com/55745404/118907891-a7c91f80-b920-11eb-8ad7-cd5c445efd82.png)
Ogni chunk possiede 8x8 voxel squares, ed una mappa contiene inizialmente 4x4 chunks 
[Implementazione Marching Squares](https://github.com/Lucaffo/Procedural-World-Generation/tree/main/Procedural%20Voxel%20Terrain%20Generator/Assets/Scenes/Marching%20Squares)


