﻿using System.Reflection;
using GeneticTanks.Extensions;
using log4net;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace GeneticTanks.Game.Components
{
  /// <summary>
  /// Renders a tank at z depth 5.
  /// </summary>
  sealed class TankRenderComponent
    : RenderComponent
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    // outline thickness on tank shapes
    private const float OutlineThickness = 0.25f;

    #region Private Fields
    // hold a reference to this component because we'll use it often
    private TankStateComponent m_state;

    // the main tank body
    private RectangleShape m_bodyShape;
    // fills the body to indicate health
    private RectangleShape m_bodyFillShape;
    // extends out from each side of the body to represent tracks
    private RectangleShape m_trackShape;
    // extends from the center of the turret to represent the gun
    private RectangleShape m_barrelShape;
    // the turret
    private CircleShape m_turretShape;
    // holds the offset of the body fill from the body itself
    private Transform m_bodyFillTransform = 
      SFML.Graphics.Transform.Identity;
    // common rendering state for all of the body pieces
    private RenderStates m_renderStates = new RenderStates
    {
      BlendMode = BlendMode.Alpha,
      Transform = SFML.Graphics.Transform.Identity
    };
    #endregion

    /// <summary>
    /// Create a tank component.
    /// </summary>
    /// <param name="parent"></param>
    public TankRenderComponent(Entity parent) 
      : base(parent)
    {
      NeedsUpdate = false;
      ZDepth = 5;
      BodyColor = Color.Red;
    }

    /// <summary>
    /// The fill color of the tank's body.
    /// </summary>
    public Color BodyColor { get; set; }

    #region RenderComponent Implementation

    public override bool Initialize()
    {
      if (!base.Initialize())
      {
        return false;
      }
      if (!RetrieveSibling(out m_state))
      {
        return false;
      }

      var size = m_state.Dimensions;
      m_bodyShape = new RectangleShape
      {
        FillColor = Color.White,
        OutlineColor = Color.Black,
        OutlineThickness = OutlineThickness,
        Size = size.ToVector2f(),
        Origin = new Vector2f(size.X / 2, size.Y / 2)
      };

      // fill is positioned at the base of the body so it acts as a health bar
      m_bodyFillShape = new RectangleShape
      {
        FillColor = BodyColor,
        Size = size.ToVector2f(),
        Origin = new Vector2f(0, size.Y / 2)
      };
      m_bodyFillTransform.Translate(-size.X / 2, 0);

      // tracks are 90% of the body length
      size.X *= 0.9f;
      size.Y += (m_state.TrackWidth * 2);
      m_trackShape = new RectangleShape
      {
        FillColor = Color.Black,
        Size = size.ToVector2f(),
        Origin = new Vector2f(size.X / 2, size.Y / 2)
      };

      var turretRadius = m_state.TurretWidth / 2;
      m_turretShape = new CircleShape
      {
        FillColor = BodyColor,
        OutlineColor = Color.Black,
        OutlineThickness = OutlineThickness,
        Radius = turretRadius,
        Origin = new Vector2f(turretRadius, turretRadius)
      };

      // make the barrel extend from the center of the turret for simplicity
      size = m_state.BarrelDimensions + new Vector2(turretRadius, 0);
      m_barrelShape = new RectangleShape
      {
        FillColor = BodyColor,
        OutlineColor = Color.Black,
        OutlineThickness = OutlineThickness,
        Size = size.ToVector2f(),
        Origin = new Vector2f(0, size.Y / 2)
      };

      Initialized = true;
      return true;
    }

    public override void Update(float deltaTime)
    {
    }

    public override void Draw(RenderTarget target)
    {
      if (!Initialized || target == null)
      {
        return;
      }

      var transform = SFML.Graphics.Transform.Identity;
      transform.Translate(Transform.Position.ToVector2f().InvertY());
      transform.Rotate(-Transform.Rotation);
      
      m_renderStates.Transform = transform;
      target.Draw(m_trackShape, m_renderStates);
      target.Draw(m_bodyShape, m_renderStates);

      m_bodyFillShape.Size = new Vector2f(
        m_state.Dimensions.X * m_state.HealthPercent,
        m_state.Dimensions.Y
        );
      m_renderStates.Transform = transform * m_bodyFillTransform;
      target.Draw(m_bodyFillShape, m_renderStates);

      m_renderStates.Transform = transform; 
      m_barrelShape.Rotation = -m_state.TurretRotation;
      target.Draw(m_barrelShape, m_renderStates);
      target.Draw(m_turretShape, m_renderStates);
    }

    #endregion
    #region IDisposable Implementation

    private bool m_disposed = false;

    protected override void Dispose(bool disposing)
    {
      if (m_disposed || !Initialized)
      {
        return;
      }

      if (disposing)
      {
        if (m_bodyShape != null)
        {
          m_bodyShape.Dispose();
        }
        if (m_bodyFillShape != null)
        {
          m_bodyFillShape.Dispose();
        }
        if (m_trackShape != null)
        {
          m_trackShape.Dispose();
        }
        if (m_barrelShape != null)
        {
          m_barrelShape.Dispose();
        }
        if (m_turretShape != null)
        {
          m_turretShape.Dispose();
        }
      }

      m_disposed = true;
      base.Dispose(disposing);
    }

    #endregion
  }
}
