using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Genetic 
{
    public List<Chromosome> chromosomes;
    public List<float> aptitudes;
    int mutationMod;
    public Genetic(int generations,int mutationThreshold)
    {
        mutationMod = mutationThreshold;

        chromosomes = new List<Chromosome>();
        aptitudes = new List<float>();

        for(int i = 0; i < generations; i++)
        {
            chromosomes.Add(new Chromosome());
        }
    }

    public float CalculateAptitude(float damageDone, float  timeAlive, float timeInSamePos, bool shot)
    {
        float damageN = damageDone * 2;
        float timeAN = timeAlive / 5;
        float shoot = shot ? 25 : 0;
        float timeSpot = (10 - timeInSamePos) >  0 ? 10 - timeInSamePos  : 0;

        float aptitude = damageN + timeAN + shoot + timeSpot;

        return aptitude;
    }

    public void AddToChromosomePool(Chromosome chromo, float aptitude)
    {
        chromosomes.Insert(0,chromo);
        aptitudes.Insert(0, aptitude);
    }

    public void Mutate()
    {
        DoCross();
        DoMutation();
    }

    void DoCross()
    {
        float totalAptitud = 0;
        List<float> ap_prob = new List<float>();

        foreach (var ap in aptitudes)
            totalAptitud += ap;

        foreach (var ap in aptitudes)
            ap_prob.Add(ap / totalAptitud);

        System.Random r = new System.Random();

        HashSet<int> selections = new HashSet<int>();

        while(selections.Count < 2)
        {
            double diceRoll = r.NextDouble();

            double cumulative = 0.0;

            for (int i = 0; i < ap_prob.Count; i++)
            {
                cumulative += ap_prob[i];
                if (diceRoll < cumulative)
                {
                    selections.Add(i);
                    break;
                }
            }
        }

        List<Chromosome> newChromo = new List<Chromosome>();

        foreach(var c1  in chromosomes)
        {
            foreach (var c2 in chromosomes)
            {
                if (c1 == c2) continue;

                newChromo.Add(Chromosome.NewCrossedChromo(c1, c2));
            }
        }

        foreach(int i in selections)
        {
            int j  = Random.Range(0, newChromo.Count);
            if(newChromo.Count >0)
            {
                chromosomes[i] = newChromo[j];
                newChromo.RemoveAt(j);
            }
        }

    }


    void DoMutation()
    {
        Chromosome.MutateChromosomeList(mutationMod, chromosomes);
    }

    public Chromosome GetChromosome()
    {
        Chromosome chromo;

        chromo = chromosomes[0];
        chromosomes.RemoveAt(0);
        if(aptitudes.Count > 0)
            aptitudes.RemoveAt(0);

        return chromo;
    }


}

[System.Serializable]
public class Chromosome: System.ICloneable
{
    [SerializeField]
    float[] values = new float[6];

    public Chromosome()
    {
        var r = new System.Random(System.DateTime.Now.Millisecond);

        for(int i = 0;i < 6 ;i++)
        {
            values[i] = (float) r.NextDouble();
        }
    }


    public static void MutateChromosomeList(int mutation, List<Chromosome> chromos)
    {
        int[] mutate = new int[Random.Range(0, mutation + 1)];
        int[] toMutate = new int[mutate.Length];

        for (int i = 0; i < mutate.Length; i++)
        {
            mutate[i] = Random.Range(0, chromos.Count);
            toMutate[i] = Random.Range(0,6);
        }

        for(int i = 0;i < mutate.Length; i++)
        {
            chromos[mutate[i]].Mutate(toMutate[i]);
        }
    }

    public void Mutate(int index)
    {
        index = index % 6;
        values[index] = Random.Range(0.0f, 1.0f);
    }

    public static Chromosome NewCrossedChromo(Chromosome c1,Chromosome c2)
    {
        int from1 = Random.Range(1, 6);
        int from2 = 6 - from1;

        Chromosome chromo = new Chromosome();

        for(int i = 0; i < from1; i++)
        {
            chromo.values[i] = c1.values[i];
        }

        for (int i = from2; i < 6; i++)
        {
            chromo.values[i] = c1.values[i];
        }

        return chromo;
    }

    public State GetState(string type, float normalized)
    {
        int counter = 0;

        float min = 100;

        if(type.ToUpper().Equals("HEALTH"))
        {
            for(int  i = 0; i  < 3;  i++)
            {
                float result = Mathf.Abs(values[i] - normalized);

                if(result < min)
                {
                    min = result;
                    counter = i % 3;
                }
            }
        }else
        {
            for (int i = 3; i < 6; i++)
            {
                float result = Mathf.Abs(values[i] - normalized);

                if (result < min)
                {
                    min = result;
                    counter = i % 3;
                }
            }
        }

        State state = State.HI;

        if (counter == 0)
            state = State.HI;
        else if (counter == 1)
            state = State.MED;
        else if (counter == 2)
            state = State.LOW;

        return state;
    }

    public object Clone()
    {
        throw new System.NotImplementedException();
    }
}
