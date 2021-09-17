// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using UnityEngine;
using LEGOMaterials;

namespace LEGOModelImporter
{
    public class InsideDetail : MonoBehaviour
    {
        public Part part;
        public PlanarField field = null;

        public void UpdateVisibility()
        {
            gameObject.SetActive(IsVisible());
        }

        private bool ShouldOptimize(PlanarFeature lhs, PlanarFeature rhs)
        {
            var connector = lhs.field.kind == ConnectionField.FieldKind.connector ? lhs : rhs;
            var receptor = lhs.field.kind == ConnectionField.FieldKind.receptor ? lhs : rhs;

            // TODO Disabled round features as they are not working as they should.
            if(connector.flags.HasFlag(PlanarFeature.Flags.squareFeature))
            {
                return receptor.flags.HasFlag(PlanarFeature.Flags.squareFeature);// || receptor.flags.HasFlag(PlanarFeature.Flags.roundFeature);
            }
/*            else if(connector.flags.HasFlag(PlanarFeature.Flags.roundFeature))
            {
                return !receptor.flags.HasFlag(PlanarFeature.Flags.squareFeature) && receptor.flags.HasFlag(PlanarFeature.Flags.roundFeature);
            }*/
            return false;
        }

        public bool IsVisible()
        {
            var quadrants = 0;
            var gridSize = field.gridSize.x * field.gridSize.y;
            
            foreach (var connected in field.connected)
            {
                var connection = field.connections[connected];
                var connectedTo = field.connectedTo[connected];
                var otherConnection = connectedTo.field.connections[connectedTo.indexOfConnection];
                if (ShouldOptimize(field.connections[connected], otherConnection))
                {
                    quadrants += connection.quadrants;
                }
            }

            return quadrants < gridSize;
        }
    }
}

