/*   
TShock, a server mod for Terraria
Copyright (C) 2011 The TShock Team

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace ChestControl
{

  // ChestDbManager ************************************************************
  public class ChestDbManager
  {
    private static readonly Chest[] Chests = new Chest[Main.maxChests];
    private IDbConnection           database;
    private static readonly String  tableName = "ChestControl";


    // ChestDbManager ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    public ChestDbManager( IDbConnection db )
    {
      Log.Write( "Initiating ChestControl...", LogLevel.Info );
      
      database = db;
      var table = new SqlTable( tableName,
                                new SqlColumn( "ChestID",  MySqlDbType.Int32 ),
                                new SqlColumn( "X",        MySqlDbType.Int32 ),
                                new SqlColumn( "Y",        MySqlDbType.Int32 ),
                                new SqlColumn( "Owner",    MySqlDbType.Text  ),     
                                new SqlColumn( "WorldID",  MySqlDbType.Text  ),   // TODO add to chest object
                                new SqlColumn( "IsLocked", MySqlDbType.Int32 ),
                                new SqlColumn( "IsRegionLocked", MySqlDbType.Int32 ),
                                new SqlColumn( "Password", MySqlDbType.Text  ),
                                new SqlColumn( "IsRefill", MySqlDbType.Int32 ),
                                new SqlColumn( "RefillDelay", MySqlDbType.Int32 )
                              );

      var dbCreator = new SqlTableCreator( db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder) new SqliteQueryCreator() : new MysqlQueryCreator());
      dbCreator.EnsureExists( table );

    } // ChestDbManager --------------------------------------------------------


    // GetChest ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    internal Chest GetChest( int id )
    {
      return Chests[id];
    } // GetChest --------------------------------------------------------------


    // LoadChests ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    public void LoadChests()
    {
      // Initialize local array of all chests in world for our tacking purposes
      for ( int i = 0; i < Chests.Length; i++ )
        Chests[i] = new Chest();

      // older versions used text file, if it exists, read that.
      if ( !LoadFromTexTFile() )
      {
        try
        {
          using ( var reader = database.QueryReader( "SELECT * FROM " + tableName + " WHERE WorldID=@0", Main.worldID.ToString() ) )
          {
            while ( reader.Read() )
            {
              int    chestID  = reader.Get<int>( "ChestID" );
              int    x        = reader.Get<int>( "X" );
              int    y        = reader.Get<int>( "Y" );
              string owner    = reader.Get<string>( "Owner" );
              int    isLocked = reader.Get<int>( "IsLocked" );
              int    isRegionLocked = reader.Get<int>( "IsRegionLocked" );
              string password = reader.Get<string>( "Password" );
              int    isRefill = reader.Get<int>( "IsRefill" );
              int    refillDelay = reader.Get<int>( "RefillDelay" );

              Chest chest = new Chest();
              chest.SetID( chestID );
              chest.SetPosition( new Vector2( x, y ) );
              chest.SetOwner( owner );
              if ( isLocked != 0 ) { chest.Lock(); }
              if ( isRegionLocked != 0 ) { chest.regionLock( true ); }
              if ( password != "" ) { chest.SetPassword( password, true ); }
              if ( isRefill != 0 ) { chest.SetRefill( true ); }

              // check if chest still exists in world
              if ( !Chest.TileIsChest( chest.GetPosition() ) )
              {
                chest.Reset(); // chest doesnt exist - so reset it
              } // if 

              // check if chest in array didn't move
              if ( !VerifyChest( chest.GetID(), chest.GetPosition() ) )
              {
                int id = Terraria.Chest.FindChest( (int) chest.GetPosition().X, (int) chest.GetPosition().Y );
                if ( id != -1 )
                  chest.SetID( id );
                else // moved, reset it
                  chest.Reset();
              } // if

              if ( Chests.Length > chest.GetID() ) { 
                Chests[chest.GetID()] = chest;
                Log.Write( "[LoadChests]: " + chest.GetOwner() + "(id:" + chest.GetID() + ")", LogLevel.Info );
              } // if

            } // while
          } // using
        } // try
        catch ( Exception ex )
        {
          Log.Write( ex.ToString(), LogLevel.Error );
        } // catch
      } // if

    } // LoadChests ------------------------------------------------------------


    // LoadFromTexTFile ++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    /* If the legacy file exists, 
     * . read legacy file
     * . populate the chest info
     * . save the chest info into the database
     * . rename the legacy file
     * . return true
     * else
     * . return false - chests will be loaded from db
     */ 
    private bool LoadFromTexTFile()
    {
      bool result = false;
      string ChestControlDirectory = Path.Combine(TShock.SavePath, "chestcontrol");
      string ChestSaveFileName = Path.Combine(ChestControlDirectory, Main.worldID + ".txt");

      if ( Directory.Exists( ChestControlDirectory ) )
      {
        if ( File.Exists( ChestSaveFileName ) )
        {
          Log.Write( "Legacy Data File Found", LogLevel.Info );

          bool error = false;
          foreach (
              var args in
                  File.ReadAllLines( ChestSaveFileName ).Select( line => line.Split( '|' ) ).Where( args => args.Length >= 7 ) )
            try
            {
              var chest = new Chest();

              // ID
              chest.SetID( int.Parse( args[0] ) );

              // Position
              chest.SetPosition( new Vector2( int.Parse( args[1] ), int.Parse( args[2] ) ) );
              
              // Owner
              chest.SetOwner( args[3] );
              
              // locked
              if ( bool.Parse( args[4] ) )
                chest.Lock();

              // region lock
              if ( bool.Parse( args[5] ) )
                chest.regionLock( true );
              
              // password
              if ( args[6] != "" )
                chest.SetPassword( args[6], true );

              // provide backwards compatibility
              if ( args.Length == 9 ) // if refill
                if ( bool.Parse( args[7] ) ) // refill
                {
                  chest.SetRefill( true );
                  // chest.SetRefillItems(args[8]); // not used - Terraria stores chest contents
                } // if

              // check if chest still exists in world
              if ( !Chest.TileIsChest( chest.GetPosition() ) )
                chest.Reset(); // chest doesnt exists - so reset it

              // check if chest in array didn't move
              if ( !VerifyChest( chest.GetID(), chest.GetPosition() ) )
              {
                int id = Terraria.Chest.FindChest( (int) chest.GetPosition().X, (int) chest.GetPosition().Y );
                if ( id != -1 )
                  chest.SetID( id );
                else
                  chest.Reset();
              } // if

              if ( Chests.Length > chest.GetID() )
              {
                Chests[chest.GetID()] = chest;
                Log.Write( "[LegacyChest]: " + chest.GetOwner() + "(id:" + chest.GetID() + ")", LogLevel.Info );
              } // if

            } // try
            catch 
            {
              error = true;
            } // catch

          if ( error )
            Log.Write( "Failed to load some chests data, corresponding chests will be left unprotected.", LogLevel.Error );

          /* Save the recently loaded chests into database
           * This will be re-done when the world exits, but if there is a crash, the rename will prevent 
           *  loading this information this way again.
           */
          SaveChests();
          File.Move( ChestSaveFileName, ChestSaveFileName + "-old.txt" );
          result = true;
        } // if - File.Exists

      } // if - dir exist
  
      return result;
    } // LoadFromTexTFile ------------------------------------------------------


    /* 
     * This coul be more robust with Add / Delete / Update
     * This model waits until server shutdown, deletes all chests 
     *  for this world from database, then writes all chests again.
     */ 
    // SaveChests ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    public void SaveChests()
    {
      try
      {
        //Log.Write( "[SaveChests] Delete All: ", LogLevel.Info );
        // Delete all chests for this world
        database.Query( "DELETE FROM " + tableName + " WHERE WorldID=@0", Main.worldID.ToString() );
      } // try
      catch ( Exception ex )
      {
        Log.Write( "DEL: " + ex.ToString(), LogLevel.Error );
      } // catch

      foreach ( Chest chest in Chests )
        if ( chest != null )
          //return; // it shouldn't EVER be null
        //else
        {
          try
          {
            if ( Chest.TileIsChest( chest.GetPosition() ) )
            {
              if ( chest.GetOwner() != "" )
              {
                Log.Write( "[SaveChests] Insert: " + chest.GetOwner() + "(id:" + chest.GetID() + ")", LogLevel.Info );
                database.Query( "INSERT INTO " + tableName + " ( ChestID, X, Y, Owner, WorldID, IsLocked, IsRegionLocked, Password, IsRefill, RefillDelay ) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9);",
                  chest.GetID(), chest.GetPosition().X, chest.GetPosition().Y, chest.GetOwner(), Main.worldID.ToString(), chest.IsLocked(), chest.IsRegionLocked(), chest.GetPassword(), chest.IsRefill(), 0 );
              } // if
            } // if
          } // try
          catch (Exception ex)
          {
            Log.Write( "[SaveChests] Insert: " + chest.GetOwner() + "(" + chest.GetID() + ") : " + ex.ToString(), LogLevel.Error );
          } // catch
        } // else

        return;
    } // SaveChests ------------------------------------------------------------


    // VerifyChest +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    private bool VerifyChest( int id, Vector2 pos )
    {
      return Terraria.Chest.FindChest( (int) pos.X, (int) pos.Y ) == id;
    } // VerifyChest -----------------------------------------------------------


  } // ChestDbManager ==========================================================


} // ChestControl **************************************************************
