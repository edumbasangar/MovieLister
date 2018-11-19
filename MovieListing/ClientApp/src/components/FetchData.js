import React, { Component } from 'react';
import throwOnError from '../utils/throwOnError';

export class FetchData extends Component {
    displayName = FetchData.name

    constructor(props) {
        super(props);
        this.state = { movies: [], loading: true };
    }

    componentDidMount() {
        this.fetchData();
    }

    fetchData = () => {
        this.setState(state => ({ ...state, isBusy: true }));
        return fetch('api/MovieData/MovieListingDetails')
            .then(throwOnError)
            .then(response => response.json())
            .then(this.onReceivedData)
            .catch(this.onError);
    }

    onError = (e) => {
        this.setState(state => ({ ...state, loading: false }));
    }

    onReceivedData = (data) => {
        this.setState({ movies: data, loading: false });
    }

    static renderMovieList(movies) {
        return (


            <table className='table'>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Name</th>
                        <th>Price</th>

                    </tr>
                </thead>
                <tbody>
                    {movies.map(movie =>
                        <tr key={movie.id}>
                            <td>{movie.id}</td>
                            <td>{movie.name}</td>
                            <td>{movie.price}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : FetchData.renderMovieList(this.state.movies);

        return (
            <div>
                <h1>Movie List</h1>
                {contents}
            </div>
        );
    }
}
