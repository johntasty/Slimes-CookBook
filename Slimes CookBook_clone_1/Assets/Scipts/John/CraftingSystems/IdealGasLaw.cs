using System;
using System.Collections.Generic;

public class IdealGasLaw 
{
    public float CalculateVolume(float pressure, float temperature)
    {
        // Convert temperature to Kelvin
        float T = temperature + 273.15f;
        
        // Calculate the volume of the container using the ideal gas law
        float R = 0.08206f; // Universal gas constant in L*atm/(mol*K)
        //ATM pressure
        float V = (1.0f * R * T) / pressure;

        // Convert volume to liters
        V /= 1000.0f;

        return V;
    }
    public Tuple<float,float, float, float> IdealGasLawTemp(float heatAdded, float molarMass, float heatCapacity, float addedmoles)
    {
        float StandarPressure = 101325f; // 1 atm in Pascals
        float initialPressure = 101325f; // 1 atm in Pascals
        float initialTemperature = 298.15f; // 25 °C in Kelvin
        float volume = 0.001f; // in cubic meters
             
        float R = 8.314f;               

        float n = initialPressure * volume / (R * initialTemperature); // moles of gas in container in kg
        n += addedmoles;
        //float mass = n * molarMass; // Mass of gas in container in kg
        
        float deltaT = heatAdded / (n * heatCapacity); // Change in temperature due to heat added
        //float energy = heatCapacity * mass * 1f;
        float finalTemperature = initialTemperature + deltaT;

        float finalPressure = initialPressure; // Start with initial pressure
        float finalVolume = volume; // Start with initial volume


        float tolerance = 0.001f;
        float maxIterations = 1000;
        int iteration = 0;

        while (iteration < maxIterations)
        {
            float deltaV = (finalVolume * finalPressure * heatAdded) / (heatCapacity * finalTemperature * (initialPressure + finalPressure));
            finalVolume = volume + deltaV;
            float newPressure = (n * R * finalTemperature) / finalVolume;

            if (MathF.Abs(newPressure - finalPressure) < tolerance)
            {
                break;
            }
            finalPressure = newPressure;
            iteration++;
        }

        float _PressureAtM = finalPressure / StandarPressure;
        return new Tuple<float, float, float,float>(_PressureAtM, finalTemperature, finalVolume ,n);

    }
    public Tuple<float, float,float> CombinedGasMixtures(List<Elements> gasNames, List<float> quantity)
    {                      
        float combinedMolarMass = 0f;
        float combinedHeatCapacity = 0f;
        float totalMass = 0f;

        for (int i = 0; i < gasNames.Count; i++)
        {
           
            totalMass += quantity[i] / 1000f;
            combinedMolarMass += quantity[i] / 1000f * gasNames[i].MollarMass;           
            combinedHeatCapacity += quantity[i] / 1000f * gasNames[i].HeatCapacity;
            
        }
       
        float molarMass = combinedMolarMass / totalMass;

        float totalMoles = totalMass / (molarMass / 1000f);

        float specificHeatCapacity = combinedHeatCapacity / (totalMass);

        return new Tuple<float, float,float>(molarMass, specificHeatCapacity, totalMoles);
    }
    
    public float CalculateAmountOfMoles(float temperature, float pressure, float volume)
    {
        float R = 8.314f; // Ideal gas constant in atm * L / (mol * K)
        float pascalsPerAtm = 101325f; // Conversion factor from atm to Pascals

        // Convert pressure to Pascals
        float pressurePa = pressure * pascalsPerAtm;

        // Calculate the number of moles using the ideal gas law
        float moles = (pressurePa * volume) / (R * temperature);

        return moles;
    }
    public Tuple<float, float> CalculateMolesofSubstance(float massCu, float molarMassCu, float massP, float molarMassP)
    {
        
        // Calculate the number of moles of Cu and P
        float molesCu = (massCu) / molarMassCu;
        float molesP = (massP) / molarMassP;

        // Calculate the number of moles of Cu3P
        float molesCu3P = MathF.Min(molesCu / 3, molesP);

        // Calculate the mass of Cu3P
        float cu3PMass = molesCu3P * (molarMassCu * 3f + molarMassP);

        return new Tuple<float, float>(molesCu3P, cu3PMass / 1000f);
    }

    public float CalculateTotalMass(List<Elements> substances, List<float> quantities)
    {
        float totalMass = 0f;

        for (int i = 0; i < substances.Count; i++)
        {
            float atomicMass = substances[i].MollarMass; // assumes GetAtomicMass function exists
            totalMass += quantities[i] /1000f * atomicMass; // convert grams to kg
        }
        return totalMass;
    }
    public float CalculatePurity(float totalMass, float massSubstance)
    {              
        float moleFraction = massSubstance / totalMass;
        float purity = moleFraction * 100.0f;
        
        return purity;
    }
}
