using System;

// Serializable so it will show up in the inspector.
[Serializable]
public class IntRange
{
	public int m_Min;       // The minimum value in this range.
	public int m_Max;       // The maximum value in this range.


	// Constructor to set the values.
	public IntRange(int min, int max)
	{
		m_Min = min;
		m_Max = max;
	}


	// Get a random value from the range.
	public int Random
	{
		get { return UnityEngine.Random.Range(m_Min, m_Max); }
	}

    public int GetMin() {
        return m_Min;
    }

    public int GetMax() {
        return m_Max;
    }

    public void SetMin(int m_Min) {
        this.m_Min = m_Min;   
    }

    public void SetMax(int m_Max) {
        this.m_Max = m_Max;
    }

}