using System.Data.SqlClient;
using PilotoIA_Backend.Models;
using System.Data;

namespace PilotoIA_Backend.DataAccess
{
    public class AuthDAO
    {
        private ConexionDAO vgBDConeccion;
        private SqlConnection oConn = new SqlConnection();
        private SqlTransaction oTran = null;

        public AuthDAO(string peDbConection)
        {
            vgBDConeccion = new ConexionDAO(peDbConection);
        }

        public async Task<MensajeRespuesta> ValidarCredenciales(string username, string password)
        {
            var result = new MensajeRespuesta();

            try
            {
                using (SqlCommand oCmC = new SqlCommand())
                {
                    oCmC.CommandType = CommandType.StoredProcedure;
                    oCmC.CommandText = "SP_ValidarUsuario"; // Cambiar al SP verdadero

                    oCmC.Parameters.AddWithValue("@vchUsername", username);
                    oCmC.Parameters.AddWithValue("@vchPassword", password);

                    oConn = await vgBDConeccion.AbrirModoLecturaAsync();
                    oTran = await Task.Run<SqlTransaction>(() => oConn.BeginTransaction());
                    oCmC.Connection = oTran.Connection;
                    oCmC.Transaction = oTran;

                    using (SqlDataReader oSqlR = await oCmC.ExecuteReaderAsync())
                    {
                        while (await oSqlR.ReadAsync())
                        {
                            result = new MensajeRespuesta()
                            {
                                Mensaje = oSqlR["Mensaje"] != DBNull.Value ? Convert.ToString(oSqlR["Mensaje"]) : string.Empty,
                                IdMensaje = oSqlR["IdMensaje"] != DBNull.Value ? Convert.ToInt32(oSqlR["IdMensaje"]) : 0,
                                IdTipoMensaje = oSqlR["TipoMensaje"] != DBNull.Value ? Convert.ToInt32(oSqlR["TipoMensaje"]) : 0,
                            };
                        }
                    }
                    oTran.Commit();
                }
            }
            catch (Exception ex)
            {
                if (oTran != null)
                {
                    await Task.Run(() => oTran.Rollback());
                }

                // Retornar mensaje de error
                return new MensajeRespuesta
                {
                    IdMensaje = -1,
                    Mensaje = "Error al validar credenciales: " + ex.Message,
                    IdTipoMensaje = 0
                };
            }
            finally
            {
                if (oTran != null)
                {
                    await oTran.DisposeAsync();
                    await oConn.DisposeAsync();
                    vgBDConeccion.Dispose();
                }
            }
            return result;
        }
    }
}