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


# Unity
 ## Componenti richiesti
** Mesh Filter ** contiene le informazioni sulla mesh.
** Mesh Render ** renderizza la mesh.

### Costruire una mesh su unity
Quando si crea una mesh su unity, ci sono dei passaggi che vanno eseguiti nel solito ordine:
1) **Assegnare i vertici**
2) **Assegnare i triangoli**

```cs
mesh.vertices = newVertices;
mesh.uv = newUV;
mesh.triangles = newTriangles;
```

### Modificare una mesh su unity
Quando si modifica, bisogna eseguiti dei passaggi con ordine:
1) **Prendere i vertici**
2) **Modificarli**
3) **Riassegnarli**

```cs
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
![](Images/Pasted%20image%2020210517012703.png)

### Colori
![](Images/Pasted%20image%2020210517012747.png)
![](Images/Pasted%20image%2020210517012856.png)
Meglio usare come byte per evitare conversioni aggiuntive.

### UV 
![](Images/Pasted%20image%2020210517012925.png)
![](Images/Pasted%20image%2020210517012934.png)

## Impostare i vertici è la parte più difficile
![](Images/Pasted%20image%2020210517013053.png)
Un esempio semplice, figurati se fosse stata una forma più complessa. E' difficile concettualizzare dove partire con gli indici dei vertici e dei triangoli.

### Mesh con numero di vertici e triangoli dinamici

![](Images/Pasted%20image%2020210517013947.png)