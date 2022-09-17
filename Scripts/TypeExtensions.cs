using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class TypeExtensions
{

    private static List<Type> s_Types = new List<Type> ()
    {
        typeof(sbyte),
        typeof(byte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(nint),
        typeof(nuint)
    };

    /// <summary> Determines whether an instance of a specified type can be assigned to a variable of the current type. </summary>
    public static bool IsCastableFrom ( this Type to, Type from )
    {

        // Check for inheritence, will also return true if the types are the same.
        if ( to.IsAssignableFrom ( from ) )
            return true;

        // Check if 'from' contains either an 'op_Implicit', or 'op_Explicit' special method that returns an object of type 'to'.
        if ( from.GetMethods ( BindingFlags.Public | BindingFlags.Static ).Any ( m => m.ReturnType == to && ( m.Name == "op_Implicit" || m.Name == "op_Explicit" ) ) )
            return true;

        // Check if 'to' contains either of the aforementioned special methods that accepts a parameter of type 'from'
        foreach ( MethodInfo method in to.GetMethods ( BindingFlags.Public | BindingFlags.Static ).Where ( m => m.ReturnType == to && ( m.Name == "op_Implicit" || m.Name == "op_Explicit" ) ) )
        {

            ParameterInfo [] info = method.GetParameters ();

            if ( info.Length > 1 )
                continue;
            if ( info [ 0 ].ParameterType == from )
                return true;
        }

        // Check if types are raw numeric types as no numeric types have conversion operators,
        // but can each be cast to every other type.
        return s_Types.Contains ( from ) && s_Types.Contains ( to );

    }

    public static bool TryCast<T> ( this object obj, out object convertedObj )
    {

        if ( !typeof ( T ).IsCastableFrom ( obj.GetType () ) )
        {
            convertedObj = default;
            return false;
        }

        if ( TryConvertNumericType<T> ( obj, out convertedObj ) )
            return true;

        MethodInfo info = obj.GetType ().GetMethods ( BindingFlags.Public | BindingFlags.Static )
            .Where ( m => m.ReturnType == typeof ( T ) && ( m.Name == "op_Implicit" || m.Name == "op_Explicit" ) )
            .FirstOrDefault ();

        if ( info == null )
            info = typeof ( T ).GetType ().GetMethods ( BindingFlags.Public | BindingFlags.Static )
                .Where ( m =>
                {
                    if ( m.ReturnType != typeof ( T ) || !( m.Name == "op_Implicit" || m.Name == "op_Explicit" ) )
                        return false;

                    ParameterInfo [] info = m.GetParameters ();

                    if ( info.Length > 1 )
                        return false;

                    if ( info [ 0 ].ParameterType != obj.GetType () )
                        return false;

                    return false;
                } ).FirstOrDefault ();

        if ( info == null )
        {
            convertedObj = default;
            return false;
        }

        object newObj = info.Invoke ( null, new object [] { obj } );

        convertedObj = (T) newObj;

        return true;

    }

    private static bool TryConvertNumericType<T> ( object obj, out object convertedObj )
    {
        Type from = obj.GetType ();
        Type to = typeof ( T );

        if ( from == typeof ( sbyte ) )
        {
            if ( to == typeof ( sbyte ) )
            {
                convertedObj = (sbyte) (sbyte) obj;
                return true;
            }
            else if ( to == typeof ( byte ) )
            {
                convertedObj = (byte) (sbyte) obj;
                return true;
            }
            else if ( to == typeof ( short ) )
            {
                convertedObj = (short) (sbyte) obj;
                return true;
            }
            else if ( to == typeof ( ushort ) )
            {
                convertedObj = (ushort) (sbyte) obj;
                return true;
            }
            else if ( to == typeof ( int ) )
            {
                convertedObj = (int) (sbyte) obj;
                return true;
            }
            else if ( to == typeof ( uint ) )
            {
                convertedObj = (uint) (sbyte) obj;
                return true;
            }
            else if ( to == typeof ( long ) )
            {
                convertedObj = (long) (sbyte) obj;
                return true;
            }
            else if ( to == typeof ( ulong ) )
            {
                convertedObj = (ulong) (sbyte) obj;
                return true;
            }
            else if ( to == typeof ( float ) )
            {
                convertedObj = (float) (sbyte) obj;
                return true;
            }
            else if ( to == typeof ( double ) )
            {
                convertedObj = (double) (sbyte) obj;
                return true;
            }
            else if ( to == typeof ( decimal ) )
            {
                convertedObj = (decimal) (sbyte) obj;
                return true;
            }
            else if ( to == typeof ( nint ) )
            {
                convertedObj = (nint) (sbyte) obj;
                return true;
            }
            else if ( to == typeof ( nuint ) )
            {
                convertedObj = (nuint) (sbyte) obj;
                return true;
            }
            else
            {
                convertedObj = default ( T );
                return false;
            }
        }
        else if ( from == typeof ( byte ) )
        {
            if ( to == typeof ( sbyte ) )
            {
                convertedObj = (sbyte) (byte) obj;
                return true;
            }
            else if ( to == typeof ( byte ) )
            {
                convertedObj = (byte) (byte) obj;
                return true;
            }
            else if ( to == typeof ( short ) )
            {
                convertedObj = (short) (byte) obj;
                return true;
            }
            else if ( to == typeof ( ushort ) )
            {
                convertedObj = (ushort) (byte) obj;
                return true;
            }
            else if ( to == typeof ( int ) )
            {
                convertedObj = (int) (byte) obj;
                return true;
            }
            else if ( to == typeof ( uint ) )
            {
                convertedObj = (uint) (byte) obj;
                return true;
            }
            else if ( to == typeof ( long ) )
            {
                convertedObj = (long) (byte) obj;
                return true;
            }
            else if ( to == typeof ( ulong ) )
            {
                convertedObj = (ulong) (byte) obj;
                return true;
            }
            else if ( to == typeof ( float ) )
            {
                convertedObj = (float) (byte) obj;
                return true;
            }
            else if ( to == typeof ( double ) )
            {
                convertedObj = (double) (byte) obj;
                return true;
            }
            else if ( to == typeof ( decimal ) )
            {
                convertedObj = (decimal) (byte) obj;
                return true;
            }
            else if ( to == typeof ( nint ) )
            {
                convertedObj = (nint) (byte) obj;
                return true;
            }
            else if ( to == typeof ( nuint ) )
            {
                convertedObj = (nuint) (byte) obj;
                return true;
            }
            else
            {
                convertedObj = default ( T );
                return false;
            }
        }
        else if ( from == typeof ( short ) )
        {
            if ( to == typeof ( sbyte ) )
            {
                convertedObj = (sbyte) (short) obj;
                return true;
            }
            else if ( to == typeof ( byte ) )
            {
                convertedObj = (byte) (short) obj;
                return true;
            }
            else if ( to == typeof ( short ) )
            {
                convertedObj = (short) (short) obj;
                return true;
            }
            else if ( to == typeof ( ushort ) )
            {
                convertedObj = (ushort) (short) obj;
                return true;
            }
            else if ( to == typeof ( int ) )
            {
                convertedObj = (int) (short) obj;
                return true;
            }
            else if ( to == typeof ( uint ) )
            {
                convertedObj = (uint) (short) obj;
                return true;
            }
            else if ( to == typeof ( long ) )
            {
                convertedObj = (long) (short) obj;
                return true;
            }
            else if ( to == typeof ( ulong ) )
            {
                convertedObj = (ulong) (short) obj;
                return true;
            }
            else if ( to == typeof ( float ) )
            {
                convertedObj = (float) (short) obj;
                return true;
            }
            else if ( to == typeof ( double ) )
            {
                convertedObj = (double) (short) obj;
                return true;
            }
            else if ( to == typeof ( decimal ) )
            {
                convertedObj = (decimal) (short) obj;
                return true;
            }
            else if ( to == typeof ( nint ) )
            {
                convertedObj = (nint) (short) obj;
                return true;
            }
            else if ( to == typeof ( nuint ) )
            {
                convertedObj = (nuint) (short) obj;
                return true;
            }
            else
            {
                convertedObj = default ( T );
                return false;
            }
        }
        else if ( from == typeof ( ushort ) )
        {
            if ( to == typeof ( sbyte ) )
            {
                convertedObj = (sbyte) (ushort) obj;
                return true;
            }
            else if ( to == typeof ( byte ) )
            {
                convertedObj = (byte) (ushort) obj;
                return true;
            }
            else if ( to == typeof ( short ) )
            {
                convertedObj = (short) (ushort) obj;
                return true;
            }
            else if ( to == typeof ( ushort ) )
            {
                convertedObj = (ushort) (ushort) obj;
                return true;
            }
            else if ( to == typeof ( int ) )
            {
                convertedObj = (int) (ushort) obj;
                return true;
            }
            else if ( to == typeof ( uint ) )
            {
                convertedObj = (uint) (ushort) obj;
                return true;
            }
            else if ( to == typeof ( long ) )
            {
                convertedObj = (long) (ushort) obj;
                return true;
            }
            else if ( to == typeof ( ulong ) )
            {
                convertedObj = (ulong) (ushort) obj;
                return true;
            }
            else if ( to == typeof ( float ) )
            {
                convertedObj = (float) (ushort) obj;
                return true;
            }
            else if ( to == typeof ( double ) )
            {
                convertedObj = (double) (ushort) obj;
                return true;
            }
            else if ( to == typeof ( decimal ) )
            {
                convertedObj = (decimal) (ushort) obj;
                return true;
            }
            else if ( to == typeof ( nint ) )
            {
                convertedObj = (nint) (ushort) obj;
                return true;
            }
            else if ( to == typeof ( nuint ) )
            {
                convertedObj = (nuint) (ushort) obj;
                return true;
            }
            else
            {
                convertedObj = default ( T );
                return false;
            }
        }
        else if ( from == typeof ( int ) )
        {
            if ( to == typeof ( sbyte ) )
            {
                convertedObj = (sbyte) (int) obj;
                return true;
            }
            else if ( to == typeof ( byte ) )
            {
                convertedObj = (byte) (int) obj;
                return true;
            }
            else if ( to == typeof ( short ) )
            {
                convertedObj = (short) (int) obj;
                return true;
            }
            else if ( to == typeof ( ushort ) )
            {
                convertedObj = (ushort) (int) obj;
                return true;
            }
            else if ( to == typeof ( int ) )
            {
                convertedObj = (int) (int) obj;
                return true;
            }
            else if ( to == typeof ( uint ) )
            {
                convertedObj = (uint) (int) obj;
                return true;
            }
            else if ( to == typeof ( long ) )
            {
                convertedObj = (long) (int) obj;
                return true;
            }
            else if ( to == typeof ( ulong ) )
            {
                convertedObj = (ulong) (int) obj;
                return true;
            }
            else if ( to == typeof ( float ) )
            {
                convertedObj = (float) (int) obj;
                return true;
            }
            else if ( to == typeof ( double ) )
            {
                convertedObj = (double) (int) obj;
                return true;
            }
            else if ( to == typeof ( decimal ) )
            {
                convertedObj = (decimal) (int) obj;
                return true;
            }
            else if ( to == typeof ( nint ) )
            {
                convertedObj = (nint) (int) obj;
                return true;
            }
            else if ( to == typeof ( nuint ) )
            {
                convertedObj = (nuint) (int) obj;
                return true;
            }
            else
            {
                convertedObj = default ( T );
                return false;
            }
        }
        else if ( from == typeof ( uint ) )
        {
            if ( to == typeof ( sbyte ) )
            {
                convertedObj = (sbyte) (uint) obj;
                return true;
            }
            else if ( to == typeof ( byte ) )
            {
                convertedObj = (byte) (uint) obj;
                return true;
            }
            else if ( to == typeof ( short ) )
            {
                convertedObj = (short) (uint) obj;
                return true;
            }
            else if ( to == typeof ( ushort ) )
            {
                convertedObj = (ushort) (uint) obj;
                return true;
            }
            else if ( to == typeof ( int ) )
            {
                convertedObj = (int) (uint) obj;
                return true;
            }
            else if ( to == typeof ( uint ) )
            {
                convertedObj = (uint) (uint) obj;
                return true;
            }
            else if ( to == typeof ( long ) )
            {
                convertedObj = (long) (uint) obj;
                return true;
            }
            else if ( to == typeof ( ulong ) )
            {
                convertedObj = (ulong) (uint) obj;
                return true;
            }
            else if ( to == typeof ( float ) )
            {
                convertedObj = (float) (uint) obj;
                return true;
            }
            else if ( to == typeof ( double ) )
            {
                convertedObj = (double) (uint) obj;
                return true;
            }
            else if ( to == typeof ( decimal ) )
            {
                convertedObj = (decimal) (uint) obj;
                return true;
            }
            else if ( to == typeof ( nint ) )
            {
                convertedObj = (nint) (uint) obj;
                return true;
            }
            else if ( to == typeof ( nuint ) )
            {
                convertedObj = (nuint) (uint) obj;
                return true;
            }
            else
            {
                convertedObj = default ( T );
                return false;
            }
        }
        else if ( from == typeof ( int ) )
        {
            if ( to == typeof ( sbyte ) )
            {
                convertedObj = (sbyte) (long) obj;
                return true;
            }
            else if ( to == typeof ( byte ) )
            {
                convertedObj = (byte) (long) obj;
                return true;
            }
            else if ( to == typeof ( short ) )
            {
                convertedObj = (short) (long) obj;
                return true;
            }
            else if ( to == typeof ( ushort ) )
            {
                convertedObj = (ushort) (long) obj;
                return true;
            }
            else if ( to == typeof ( int ) )
            {
                convertedObj = (int) (long) obj;
                return true;
            }
            else if ( to == typeof ( uint ) )
            {
                convertedObj = (uint) (long) obj;
                return true;
            }
            else if ( to == typeof ( long ) )
            {
                convertedObj = (long) (long) obj;
                return true;
            }
            else if ( to == typeof ( ulong ) )
            {
                convertedObj = (ulong) (long) obj;
                return true;
            }
            else if ( to == typeof ( float ) )
            {
                convertedObj = (float) (long) obj;
                return true;
            }
            else if ( to == typeof ( double ) )
            {
                convertedObj = (double) (long) obj;
                return true;
            }
            else if ( to == typeof ( decimal ) )
            {
                convertedObj = (decimal) (long) obj;
                return true;
            }
            else if ( to == typeof ( nint ) )
            {
                convertedObj = (nint) (long) obj;
                return true;
            }
            else if ( to == typeof ( nuint ) )
            {
                convertedObj = (nuint) (long) obj;
                return true;
            }
            else
            {
                convertedObj = default ( T );
                return false;
            }
        }
        else if ( from == typeof ( ulong ) )
        {
            if ( to == typeof ( sbyte ) )
            {
                convertedObj = (sbyte) (ulong) obj;
                return true;
            }
            else if ( to == typeof ( byte ) )
            {
                convertedObj = (byte) (ulong) obj;
                return true;
            }
            else if ( to == typeof ( short ) )
            {
                convertedObj = (short) (ulong) obj;
                return true;
            }
            else if ( to == typeof ( ushort ) )
            {
                convertedObj = (ushort) (ulong) obj;
                return true;
            }
            else if ( to == typeof ( int ) )
            {
                convertedObj = (int) (ulong) obj;
                return true;
            }
            else if ( to == typeof ( uint ) )
            {
                convertedObj = (uint) (ulong) obj;
                return true;
            }
            else if ( to == typeof ( long ) )
            {
                convertedObj = (long) (ulong) obj;
                return true;
            }
            else if ( to == typeof ( ulong ) )
            {
                convertedObj = (ulong) (ulong) obj;
                return true;
            }
            else if ( to == typeof ( float ) )
            {
                convertedObj = (float) (ulong) obj;
                return true;
            }
            else if ( to == typeof ( double ) )
            {
                convertedObj = (double) (ulong) obj;
                return true;
            }
            else if ( to == typeof ( decimal ) )
            {
                convertedObj = (decimal) (ulong) obj;
                return true;
            }
            else if ( to == typeof ( nint ) )
            {
                convertedObj = (nint) (ulong) obj;
                return true;
            }
            else if ( to == typeof ( nuint ) )
            {
                convertedObj = (nuint) (ulong) obj;
                return true;
            }
            else
            {
                convertedObj = default ( T );
                return false;
            }
        }
        else if ( from == typeof ( float ) )
        {
            if ( to == typeof ( sbyte ) )
            {
                convertedObj = (sbyte) (float) obj;
                return true;
            }
            else if ( to == typeof ( byte ) )
            {
                convertedObj = (byte) (float) obj;
                return true;
            }
            else if ( to == typeof ( short ) )
            {
                convertedObj = (short) (float) obj;
                return true;
            }
            else if ( to == typeof ( ushort ) )
            {
                convertedObj = (ushort) (float) obj;
                return true;
            }
            else if ( to == typeof ( int ) )
            {
                convertedObj = (int) (float) obj;
                return true;
            }
            else if ( to == typeof ( uint ) )
            {
                convertedObj = (uint) (float) obj;
                return true;
            }
            else if ( to == typeof ( long ) )
            {
                convertedObj = (long) (float) obj;
                return true;
            }
            else if ( to == typeof ( ulong ) )
            {
                convertedObj = (ulong) (float) obj;
                return true;
            }
            else if ( to == typeof ( float ) )
            {
                convertedObj = (float) (float) obj;
                return true;
            }
            else if ( to == typeof ( double ) )
            {
                convertedObj = (double) (float) obj;
                return true;
            }
            else if ( to == typeof ( decimal ) )
            {
                convertedObj = (decimal) (float) obj;
                return true;
            }
            else if ( to == typeof ( nint ) )
            {
                convertedObj = (nint) (float) obj;
                return true;
            }
            else if ( to == typeof ( nuint ) )
            {
                convertedObj = (nuint) (float) obj;
                return true;
            }
            else
            {
                convertedObj = default ( T );
                return false;
            }
        }
        else if ( from == typeof ( double ) )
        {
            if ( to == typeof ( sbyte ) )
            {
                convertedObj = (sbyte) (double) obj;
                return true;
            }
            else if ( to == typeof ( byte ) )
            {
                convertedObj = (byte) (double) obj;
                return true;
            }
            else if ( to == typeof ( short ) )
            {
                convertedObj = (short) (double) obj;
                return true;
            }
            else if ( to == typeof ( ushort ) )
            {
                convertedObj = (ushort) (double) obj;
                return true;
            }
            else if ( to == typeof ( int ) )
            {
                convertedObj = (int) (double) obj;
                return true;
            }
            else if ( to == typeof ( uint ) )
            {
                convertedObj = (uint) (double) obj;
                return true;
            }
            else if ( to == typeof ( long ) )
            {
                convertedObj = (long) (double) obj;
                return true;
            }
            else if ( to == typeof ( ulong ) )
            {
                convertedObj = (ulong) (double) obj;
                return true;
            }
            else if ( to == typeof ( float ) )
            {
                convertedObj = (float) (double) obj;
                return true;
            }
            else if ( to == typeof ( double ) )
            {
                convertedObj = (double) (double) obj;
                return true;
            }
            else if ( to == typeof ( decimal ) )
            {
                convertedObj = (decimal) (double) obj;
                return true;
            }
            else if ( to == typeof ( nint ) )
            {
                convertedObj = (nint) (double) obj;
                return true;
            }
            else if ( to == typeof ( nuint ) )
            {
                convertedObj = (nuint) (double) obj;
                return true;
            }
            else
            {
                convertedObj = default ( T );
                return false;
            }
        }
        else if ( from == typeof ( decimal ) )
        {
            if ( to == typeof ( sbyte ) )
            {
                convertedObj = (sbyte) (decimal) obj;
                return true;
            }
            else if ( to == typeof ( byte ) )
            {
                convertedObj = (byte) (decimal) obj;
                return true;
            }
            else if ( to == typeof ( short ) )
            {
                convertedObj = (short) (decimal) obj;
                return true;
            }
            else if ( to == typeof ( ushort ) )
            {
                convertedObj = (ushort) (decimal) obj;
                return true;
            }
            else if ( to == typeof ( int ) )
            {
                convertedObj = (int) (decimal) obj;
                return true;
            }
            else if ( to == typeof ( uint ) )
            {
                convertedObj = (uint) (decimal) obj;
                return true;
            }
            else if ( to == typeof ( long ) )
            {
                convertedObj = (long) (decimal) obj;
                return true;
            }
            else if ( to == typeof ( ulong ) )
            {
                convertedObj = (ulong) (decimal) obj;
                return true;
            }
            else if ( to == typeof ( float ) )
            {
                convertedObj = (float) (decimal) obj;
                return true;
            }
            else if ( to == typeof ( double ) )
            {
                convertedObj = (double) (decimal) obj;
                return true;
            }
            else if ( to == typeof ( decimal ) )
            {
                convertedObj = (decimal) (decimal) obj;
                return true;
            }
            else if ( to == typeof ( nint ) )
            {
                convertedObj = (nint) (decimal) obj;
                return true;
            }
            else if ( to == typeof ( nuint ) )
            {
                convertedObj = (nuint) (decimal) obj;
                return true;
            }
            else
            {
                convertedObj = default ( T );
                return false;
            }
        }
        else if ( from == typeof ( nint ) )
        {
            if ( to == typeof ( sbyte ) )
            {
                convertedObj = (sbyte) (nint) obj;
                return true;
            }
            else if ( to == typeof ( byte ) )
            {
                convertedObj = (byte) (nint) obj;
                return true;
            }
            else if ( to == typeof ( short ) )
            {
                convertedObj = (short) (nint) obj;
                return true;
            }
            else if ( to == typeof ( ushort ) )
            {
                convertedObj = (ushort) (nint) obj;
                return true;
            }
            else if ( to == typeof ( int ) )
            {
                convertedObj = (int) (nint) obj;
                return true;
            }
            else if ( to == typeof ( uint ) )
            {
                convertedObj = (uint) (nint) obj;
                return true;
            }
            else if ( to == typeof ( long ) )
            {
                convertedObj = (long) (nint) obj;
                return true;
            }
            else if ( to == typeof ( ulong ) )
            {
                convertedObj = (ulong) (nint) obj;
                return true;
            }
            else if ( to == typeof ( float ) )
            {
                convertedObj = (float) (nint) obj;
                return true;
            }
            else if ( to == typeof ( double ) )
            {
                convertedObj = (double) (nint) obj;
                return true;
            }
            else if ( to == typeof ( decimal ) )
            {
                convertedObj = (decimal) (nint) obj;
                return true;
            }
            else if ( to == typeof ( nint ) )
            {
                convertedObj = (nint) (nint) obj;
                return true;
            }
            else if ( to == typeof ( nuint ) )
            {
                convertedObj = (nuint) (nint) obj;
                return true;
            }
            else
            {
                convertedObj = default ( T );
                return false;
            }
        }
        else if ( from == typeof ( nuint ) )
        {
            if ( to == typeof ( sbyte ) )
            {
                convertedObj = (sbyte) (nuint) obj;
                return true;
            }
            else if ( to == typeof ( byte ) )
            {
                convertedObj = (byte) (nuint) obj;
                return true;
            }
            else if ( to == typeof ( short ) )
            {
                convertedObj = (short) (nuint) obj;
                return true;
            }
            else if ( to == typeof ( ushort ) )
            {
                convertedObj = (ushort) (nuint) obj;
                return true;
            }
            else if ( to == typeof ( int ) )
            {
                convertedObj = (int) (nuint) obj;
                return true;
            }
            else if ( to == typeof ( uint ) )
            {
                convertedObj = (uint) (nuint) obj;
                return true;
            }
            else if ( to == typeof ( long ) )
            {
                convertedObj = (long) (nuint) obj;
                return true;
            }
            else if ( to == typeof ( ulong ) )
            {
                convertedObj = (ulong) (nuint) obj;
                return true;
            }
            else if ( to == typeof ( float ) )
            {
                convertedObj = (float) (nuint) obj;
                return true;
            }
            else if ( to == typeof ( double ) )
            {
                convertedObj = (double) (nuint) obj;
                return true;
            }
            else if ( to == typeof ( decimal ) )
            {
                convertedObj = (decimal) (nuint) obj;
                return true;
            }
            else if ( to == typeof ( nint ) )
            {
                convertedObj = (nint) (nuint) obj;
                return true;
            }
            else if ( to == typeof ( nuint ) )
            {
                convertedObj = (nuint) (nuint) obj;
                return true;
            }
            else
            {
                convertedObj = default ( T );
                return false;
            }
        }
        else
        {
            convertedObj = default ( T );
            return false;
        }
    }
}
