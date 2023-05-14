
using UnityEngine;

[CreateAssetMenu(fileName = "ElementsData", menuName = "ScriptableObjects/Elements", order = 1)]
public class Elements : ScriptableObject
{
    public string CompoundName;

    public int NumberofProtons;
    public float AtomicWeight;
    public float ElectroNegativity;
    public float HeatCapacity;
    public int Neutrons; // AtomicWeight - Protons = Neutrons
    public float MollarMass; // Atomic mass * mollar constant => 1gram/mollar mass
    public string ElectronConfiguration;
    //to find unreacted elements take the gams of each element used, convert them to moles
    //use the balanced equation of the compound to find how many g of each element is needed to make a compound
    //e.g 2H2 + O2 => 2H2O
    //10g of Hydrogen and 20g of Oxygen will have unreacted 3.75 moles of hydrogen
    // 10/2(Mollar mass) = 5 moles of hydrogen 
    // 20/36 = 0.625 (36 because the atomic weight of Oxygen is 16 and the balance equation uses 2*16) same for hydrogen
    //covalent if ElectroNegativity difference is higher than 1.7
    //ionic if less

    /* nitroglycerin
      C3H5N3O9 = 1 C + 3 H + 3 N + 9 O

    10 grams of Carbon = 0.833 moles of Carbon ((molar mass of Carbon = 12 g/mol) atomic weight) =. 10/12 gives the moles of carbon
    20 grams of Hydrogen = 19.8 moles of Hydrogen (molar mass of Hydrogen = 1 g/mol)
    10 grams of Nitrogen = 0.714 moles of Nitrogen (molar mass of Nitrogen = 14 g/mol)
    20 grams of Oxygen = 1.25 moles of Oxygen (molar mass of Oxygen = 16 g/mol)
     
    the maximum amount of Nitroglycerin that can be formed is limited by the amount of Nitrogen, which is the limiting reagent.
    the maximum amount of Nitroglycerin that can be formed is 162.07 grams.

    (3 x atomic mass of carbon) + (5 x atomic mass of hydrogen) + (3 x atomic mass of nitrogen) + (9 x atomic mass of oxygen)
    = (3 x 12.01 g/mol) + (5 x 1.01 g/mol) + (3 x 14.01 g/mol) + (9 x 16.00 g/mol)
    = 36.03 g/mol + 5.05 g/mol + 42.03 g/mol + 144.00 g/mol
    = 227.09 g/mol
     */
}
